using System.Threading.Channels;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Events;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Runtime.Implementations;

/// <summary>
/// Implementation of pipeline runtime that manages execution lifecycle.
/// </summary>
public sealed class PipelineRuntime : IPipelineRuntime
{
    private readonly IPipelineDefinition _definition;
    private readonly Dictionary<IPipelineLinkDefinition, object> _channels = new();
    private readonly Dictionary<IPipelineElementDefinition, ElementRuntimeContext> _contexts = new();
    private readonly List<Task> _elementTasks = new();
    private readonly CancellationTokenSource _internalCts = new();
    
    private PipelineState _state = PipelineState.Created;
    private readonly object _stateLock = new();

    public PipelineState State
    {
        get
        {
            lock (_stateLock)
            {
                return _state;
            }
        }
    }

    public IPipelineBus Bus { get; }

    public PipelineRuntime(IPipelineDefinition definition, IPipelineBus? bus = null)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Bus = bus ?? new PipelineBus();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        lock (_stateLock)
        {
            if (_state != PipelineState.Created && _state != PipelineState.Prepared)
                throw new InvalidOperationException($"Cannot start pipeline in state {_state}.");

            if (_state == PipelineState.Created)
            {
                Prepare();
            }

            SetState(PipelineState.Running);
        }

        // Start all element processors
        foreach (var element in _definition.Elements)
        {
            var context = _contexts[element];
            var degreeOfParallelism = element.Policy?.DegreeOfParallelism ?? 1;

            Bus.Publish(new ElementStartedEvent(element.Name));

            // Track completion and faults per element to avoid race conditions
            var completionSent = 0; // 0 = not sent, 1 = sent
            var faultSent = 0; // 0 = not sent, 1 = sent

            // Spawn processors according to DegreeOfParallelism
            for (int i = 0; i < degreeOfParallelism; i++)
            {
                var processor = element.ProcessorFactory.CreateProcessor();

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await processor.RunAsync(context, _internalCts.Token);
                        
                        // Only publish completion event once per element using atomic operation
                        if (Interlocked.CompareExchange(ref completionSent, 1, 0) == 0)
                        {
                            Bus.Publish(new ElementCompletedEvent(element.Name));
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        // Only publish first fault event per element using atomic operation
                        if (Interlocked.CompareExchange(ref faultSent, 1, 0) == 0)
                        {
                            Bus.Publish(new ElementFaultedEvent(element.Name, ex));
                            SetState(PipelineState.Faulted);
                        }
                        // Cancel remaining elements for controlled shutdown (idempotent)
                        _internalCts.Cancel();
                        // Do not rethrow - errors are communicated via events
                    }
                }, cancellationToken);

                _elementTasks.Add(task);
            }
        }

        await Task.CompletedTask;
    }

    public async Task DrainAsync(CancellationToken cancellationToken)
    {
        lock (_stateLock)
        {
            if (_state != PipelineState.Running)
                throw new InvalidOperationException($"Cannot drain pipeline in state {_state}.");

            SetState(PipelineState.Draining);
        }

        // Elements manage their own output completion.
        // We just wait for all elements to naturally finish processing.
        // Source elements will complete when their IAsyncEnumerable completes.
        // Other elements complete their outputs when their inputs complete.
        
        // Wait for all element tasks to complete
        await Task.WhenAll(_elementTasks);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_stateLock)
        {
            if (_state == PipelineState.Stopped || _state == PipelineState.Faulted)
                return;
        }

        _internalCts.Cancel();

        try
        {
            await Task.WhenAll(_elementTasks);
        }
        catch (OperationCanceledException)
        {
            // Expected during cancellation
        }
        catch (Exception ex)
        {
            Bus.Publish(new PipelineErrorEvent(ex));
        }

        SetState(PipelineState.Stopped);
    }

    public async ValueTask DisposeAsync()
    {
        if (_state == PipelineState.Running || _state == PipelineState.Draining)
        {
            await StopAsync(CancellationToken.None);
        }

        _internalCts.Dispose();
        
        if (Bus is IDisposable disposableBus)
        {
            disposableBus.Dispose();
        }
    }

    private void Prepare()
    {
        var sharedContext = new PipelineContext();

        // Create channels for all links using a helper method that avoids reflection
        foreach (var link in _definition.Links)
        {
            var options = new BoundedChannelOptions(link.Policy.BufferSize)
            {
                FullMode = link.Policy.FullMode
            };

            var channel = CreateBoundedChannel(link.Source.DataType, options);
            _channels[link] = channel;
        }

        // Create contexts for all elements and wire up pads
        foreach (var element in _definition.Elements)
        {
            var context = new ElementRuntimeContext(Bus, sharedContext);

            // Wire up input pads to channel readers
            foreach (var input in element.Inputs)
            {
                var link = _definition.Links.FirstOrDefault(l => l.Target == input);
                if (link != null && _channels.TryGetValue(link, out var channelObj))
                {
                    RegisterInputReader(context, input.Name, input.DataType, channelObj);
                }
            }

            // Wire up output pads to channel writers
            foreach (var output in element.Outputs)
            {
                var link = _definition.Links.FirstOrDefault(l => l.Source == output);
                if (link != null && _channels.TryGetValue(link, out var channelObj))
                {
                    RegisterOutputWriter(context, output.Name, output.DataType, channelObj);
                }
            }

            _contexts[element] = context;
        }

        SetState(PipelineState.Prepared);
    }

    private static object CreateBoundedChannel(Type dataType, BoundedChannelOptions options)
    {
        // Use reflection only once during Prepare(), not in hot path
        var channelType = typeof(Channel<>).MakeGenericType(dataType);
        var createBoundedMethod = typeof(Channel).GetMethod(nameof(Channel.CreateBounded), new[] { typeof(BoundedChannelOptions) });
        var genericCreateBounded = createBoundedMethod!.MakeGenericMethod(dataType);
        return genericCreateBounded.Invoke(null, new object[] { options })!;
    }

    private static void RegisterInputReader(ElementRuntimeContext context, string padName, Type dataType, object channelObj)
    {
        // Use reflection only once during Prepare(), not in hot path
        var readerProp = channelObj.GetType().GetProperty("Reader");
        var reader = readerProp!.GetValue(channelObj)!;

        var registerMethod = typeof(ElementRuntimeContext)
            .GetMethod(nameof(ElementRuntimeContext.RegisterInputReader))!
            .MakeGenericMethod(dataType);

        registerMethod.Invoke(context, new[] { padName, reader });
    }

    private static void RegisterOutputWriter(ElementRuntimeContext context, string padName, Type dataType, object channelObj)
    {
        // Use reflection only once during Prepare(), not in hot path
        var writerProp = channelObj.GetType().GetProperty("Writer");
        var writer = writerProp!.GetValue(channelObj)!;

        var registerMethod = typeof(ElementRuntimeContext)
            .GetMethod(nameof(ElementRuntimeContext.RegisterOutputWriter))!
            .MakeGenericMethod(dataType);

        registerMethod.Invoke(context, new[] { padName, writer });
    }

    private void SetState(PipelineState newState)
    {
        PipelineState oldState;
        lock (_stateLock)
        {
            oldState = _state;
            _state = newState;
        }
        Bus.Publish(new PipelineStateChangedEvent(oldState, newState));
    }
}
