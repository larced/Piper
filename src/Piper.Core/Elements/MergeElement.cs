using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Elements;

/// <summary>
/// Factory for creating merge element processors.
/// </summary>
public sealed class MergeElementProcessorFactory<T> : IElementProcessorFactory
{
    private readonly int _inputCount;
    private readonly IMergePolicy _policy;

    public MergeElementProcessorFactory(int inputCount, IMergePolicy policy)
    {
        if (inputCount < 2)
            throw new ArgumentException("Merge element must have at least 2 inputs.", nameof(inputCount));

        _inputCount = inputCount;
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    public IElementProcessor CreateProcessor()
    {
        return new MergeElementProcessor<T>(_inputCount, _policy);
    }
}

/// <summary>
/// Processor for merge elements that combine multiple inputs into one output.
/// </summary>
internal sealed class MergeElementProcessor<T> : IElementProcessor
{
    private readonly int _inputCount;
    private readonly IMergePolicy _policy;

    public MergeElementProcessor(int inputCount, IMergePolicy policy)
    {
        _inputCount = inputCount;
        _policy = policy;
    }

    public async Task RunAsync(IElementRuntimeContext context, CancellationToken cancellationToken)
    {
        var readers = Enumerable.Range(0, _inputCount)
            .Select(i => context.GetInputReader<T>($"input{i}"))
            .ToList();
        var writer = context.GetOutputWriter<T>("output");

        try
        {
            switch (_policy.Mode)
            {
                case MergeMode.Interleave:
                    await InterleaveAsync(readers, writer, cancellationToken);
                    break;
                case MergeMode.Zip:
                    await ZipAsync(readers, writer, cancellationToken);
                    break;
                case MergeMode.Priority:
                    await PriorityAsync(readers, writer, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Merge mode {_policy.Mode} is not supported.");
            }
        }
        finally
        {
            writer.Complete();
        }
    }

    private static async Task InterleaveAsync(
        List<System.Threading.Channels.ChannelReader<T>> readers,
        System.Threading.Channels.ChannelWriter<T> writer,
        CancellationToken cancellationToken)
    {
        var tasks = readers.Select(async reader =>
        {
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                await writer.WriteAsync(item, cancellationToken);
            }
        }).ToList();

        await Task.WhenAll(tasks);
    }

    private static async Task ZipAsync(
        List<System.Threading.Channels.ChannelReader<T>> readers,
        System.Threading.Channels.ChannelWriter<T> writer,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var items = new List<T>();
            var allSucceeded = true;

            foreach (var reader in readers)
            {
                if (await reader.WaitToReadAsync(cancellationToken))
                {
                    if (reader.TryRead(out var item))
                    {
                        items.Add(item);
                    }
                    else
                    {
                        allSucceeded = false;
                        break;
                    }
                }
                else
                {
                    // One of the readers is complete
                    return;
                }
            }

            if (!allSucceeded)
                break;

            // Write all items in order
            foreach (var item in items)
            {
                await writer.WriteAsync(item, cancellationToken);
            }
        }
    }

    private static async Task PriorityAsync(
        List<System.Threading.Channels.ChannelReader<T>> readers,
        System.Threading.Channels.ChannelWriter<T> writer,
        CancellationToken cancellationToken)
    {
        // Priority merge: read from inputs in order, prioritizing lower-index inputs
        var activeReaders = readers.ToList();

        while (activeReaders.Count > 0)
        {
            var itemWritten = false;

            foreach (var reader in activeReaders.ToList())
            {
                if (reader.TryRead(out var item))
                {
                    await writer.WriteAsync(item, cancellationToken);
                    itemWritten = true;
                    break; // Start over with highest priority
                }
                else if (!await reader.WaitToReadAsync(cancellationToken))
                {
                    // This reader is complete
                    activeReaders.Remove(reader);
                }
            }

            if (!itemWritten && activeReaders.Count > 0)
            {
                // Wait for any reader to have data
                await Task.Delay(1, cancellationToken);
            }
        }
    }
}

/// <summary>
/// Helper class for creating merge element definitions.
/// </summary>
public static class MergeElement
{
    public static IPipelineElementDefinition Create<T>(
        string name,
        int inputCount,
        IMergePolicy? policy = null)
    {
        if (inputCount < 2)
            throw new ArgumentException("Merge element must have at least 2 inputs.", nameof(inputCount));

        var inputs = Enumerable.Range(0, inputCount)
            .Select(i => new InputPadDefinition($"input{i}", typeof(T)))
            .ToArray();
        var outputs = new[] { new OutputPadDefinition("output", typeof(T)) };

        var mergePolicy = policy ?? MergePolicy.Default;
        var factory = new MergeElementProcessorFactory<T>(inputCount, mergePolicy);

        return new PipelineElementDefinition(name, inputs, outputs, factory, mergePolicy);
    }
}
