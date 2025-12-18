using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Elements;

/// <summary>
/// Factory for creating transform element processors.
/// </summary>
public sealed class TransformElementProcessorFactory<TIn, TOut> : IElementProcessorFactory
{
    private readonly Func<TIn, CancellationToken, Task<TOut>> _transformFunc;

    public TransformElementProcessorFactory(Func<TIn, CancellationToken, Task<TOut>> transformFunc)
    {
        _transformFunc = transformFunc ?? throw new ArgumentNullException(nameof(transformFunc));
    }

    public IElementProcessor CreateProcessor()
    {
        return new TransformElementProcessor<TIn, TOut>(_transformFunc);
    }
}

/// <summary>
/// Processor for transform elements that process items one-by-one.
/// </summary>
internal sealed class TransformElementProcessor<TIn, TOut> : IElementProcessor
{
    private readonly Func<TIn, CancellationToken, Task<TOut>> _transformFunc;

    public TransformElementProcessor(Func<TIn, CancellationToken, Task<TOut>> transformFunc)
    {
        _transformFunc = transformFunc;
    }

    public async Task RunAsync(IElementRuntimeContext context, CancellationToken cancellationToken)
    {
        var reader = context.GetInputReader<TIn>("input");
        var writer = context.GetOutputWriter<TOut>("output");

        try
        {
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                var result = await _transformFunc(item, cancellationToken);
                await writer.WriteAsync(result, cancellationToken);
            }
        }
        finally
        {
            writer.Complete();
        }
    }
}

/// <summary>
/// Helper class for creating transform element definitions.
/// </summary>
public static class TransformElement
{
    public static IPipelineElementDefinition Create<TIn, TOut>(
        string name,
        Func<TIn, CancellationToken, Task<TOut>> transformFunc,
        IElementPolicy? policy = null)
    {
        var inputs = new[] { new InputPadDefinition("input", typeof(TIn)) };
        var outputs = new[] { new OutputPadDefinition("output", typeof(TOut)) };
        var factory = new TransformElementProcessorFactory<TIn, TOut>(transformFunc);

        return new PipelineElementDefinition(name, inputs, outputs, factory, policy);
    }

    public static IPipelineElementDefinition Create<TIn, TOut>(
        string name,
        Func<TIn, TOut> transformFunc,
        IElementPolicy? policy = null)
    {
        return Create<TIn, TOut>(name, (item, _) => Task.FromResult(transformFunc(item)), policy);
    }
}
