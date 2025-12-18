using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Elements;

/// <summary>
/// Factory for creating source element processors.
/// </summary>
public sealed class SourceElementProcessorFactory<T> : IElementProcessorFactory
{
    private readonly Func<CancellationToken, IAsyncEnumerable<T>> _sourceFunc;

    public SourceElementProcessorFactory(Func<CancellationToken, IAsyncEnumerable<T>> sourceFunc)
    {
        _sourceFunc = sourceFunc ?? throw new ArgumentNullException(nameof(sourceFunc));
    }

    public IElementProcessor CreateProcessor()
    {
        return new SourceElementProcessor<T>(_sourceFunc);
    }
}

/// <summary>
/// Processor for source elements that generate items.
/// </summary>
internal sealed class SourceElementProcessor<T> : IElementProcessor
{
    private readonly Func<CancellationToken, IAsyncEnumerable<T>> _sourceFunc;

    public SourceElementProcessor(Func<CancellationToken, IAsyncEnumerable<T>> sourceFunc)
    {
        _sourceFunc = sourceFunc;
    }

    public async Task RunAsync(IElementRuntimeContext context, CancellationToken cancellationToken)
    {
        var writer = context.GetOutputWriter<T>("output");

        try
        {
            await foreach (var item in _sourceFunc(cancellationToken).WithCancellation(cancellationToken))
            {
                await writer.WriteAsync(item, cancellationToken);
            }
        }
        finally
        {
            writer.Complete();
        }
    }
}

/// <summary>
/// Helper class for creating source element definitions.
/// </summary>
public static class SourceElement
{
    public static IPipelineElementDefinition Create<T>(
        string name,
        Func<CancellationToken, IAsyncEnumerable<T>> sourceFunc,
        IElementPolicy? policy = null)
    {
        var outputs = new[] { new OutputPadDefinition("output", typeof(T)) };
        var factory = new SourceElementProcessorFactory<T>(sourceFunc);

        return new PipelineElementDefinition(name, Array.Empty<IInputPadDefinition>(), outputs, factory, policy);
    }
}
