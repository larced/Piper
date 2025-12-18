using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Elements;

/// <summary>
/// Factory for creating sink element processors.
/// </summary>
public sealed class SinkElementProcessorFactory<T> : IElementProcessorFactory
{
    private readonly Func<T, CancellationToken, Task> _sinkFunc;

    public SinkElementProcessorFactory(Func<T, CancellationToken, Task> sinkFunc)
    {
        _sinkFunc = sinkFunc ?? throw new ArgumentNullException(nameof(sinkFunc));
    }

    public IElementProcessor CreateProcessor()
    {
        return new SinkElementProcessor<T>(_sinkFunc);
    }
}

/// <summary>
/// Processor for sink elements that consume items.
/// </summary>
internal sealed class SinkElementProcessor<T> : IElementProcessor
{
    private readonly Func<T, CancellationToken, Task> _sinkFunc;

    public SinkElementProcessor(Func<T, CancellationToken, Task> sinkFunc)
    {
        _sinkFunc = sinkFunc;
    }

    public async Task RunAsync(IElementRuntimeContext context, CancellationToken cancellationToken)
    {
        var reader = context.GetInputReader<T>("input");

        await foreach (var item in reader.ReadAllAsync(cancellationToken))
        {
            await _sinkFunc(item, cancellationToken);
        }
    }
}

/// <summary>
/// Helper class for creating sink element definitions.
/// </summary>
public static class SinkElement
{
    public static IPipelineElementDefinition Create<T>(
        string name,
        Func<T, CancellationToken, Task> sinkFunc,
        IElementPolicy? policy = null)
    {
        var inputs = new[] { new InputPadDefinition("input", typeof(T)) };
        var factory = new SinkElementProcessorFactory<T>(sinkFunc);

        return new PipelineElementDefinition(name, inputs, Array.Empty<IOutputPadDefinition>(), factory, policy);
    }

    public static IPipelineElementDefinition Create<T>(
        string name,
        Action<T> sinkAction,
        IElementPolicy? policy = null)
    {
        return Create<T>(name, (item, _) => { sinkAction(item); return Task.CompletedTask; }, policy);
    }
}
