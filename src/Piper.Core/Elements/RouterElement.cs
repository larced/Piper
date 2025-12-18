using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Elements;

/// <summary>
/// Factory for creating router element processors.
/// </summary>
public sealed class RouterElementProcessorFactory<T> : IElementProcessorFactory
{
    private readonly int _outputCount;
    private readonly Func<T, int> _routeFunc;

    public RouterElementProcessorFactory(int outputCount, Func<T, int> routeFunc)
    {
        if (outputCount < 2)
            throw new ArgumentException("Router element must have at least 2 outputs.", nameof(outputCount));

        _outputCount = outputCount;
        _routeFunc = routeFunc ?? throw new ArgumentNullException(nameof(routeFunc));
    }

    public IElementProcessor CreateProcessor()
    {
        return new RouterElementProcessor<T>(_outputCount, _routeFunc);
    }
}

/// <summary>
/// Processor for router elements that route items to one of multiple outputs.
/// </summary>
internal sealed class RouterElementProcessor<T> : IElementProcessor
{
    private readonly int _outputCount;
    private readonly Func<T, int> _routeFunc;

    public RouterElementProcessor(int outputCount, Func<T, int> routeFunc)
    {
        _outputCount = outputCount;
        _routeFunc = routeFunc;
    }

    public async Task RunAsync(IElementRuntimeContext context, CancellationToken cancellationToken)
    {
        var reader = context.GetInputReader<T>("input");
        var writers = Enumerable.Range(0, _outputCount)
            .Select(i => context.GetOutputWriter<T>($"output{i}"))
            .ToList();

        try
        {
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                var outputIndex = _routeFunc(item);
                
                if (outputIndex < 0 || outputIndex >= _outputCount)
                {
                    throw new InvalidOperationException(
                        $"Route function returned invalid output index {outputIndex}. " +
                        $"Expected value between 0 and {_outputCount - 1}.");
                }

                await writers[outputIndex].WriteAsync(item, cancellationToken);
            }
        }
        finally
        {
            foreach (var writer in writers)
            {
                writer.Complete();
            }
        }
    }
}

/// <summary>
/// Helper class for creating router element definitions.
/// </summary>
public static class RouterElement
{
    public static IPipelineElementDefinition Create<T>(
        string name,
        int outputCount,
        Func<T, int> routeFunc,
        IElementPolicy? policy = null)
    {
        if (outputCount < 2)
            throw new ArgumentException("Router element must have at least 2 outputs.", nameof(outputCount));

        var inputs = new[] { new InputPadDefinition("input", typeof(T)) };
        var outputs = Enumerable.Range(0, outputCount)
            .Select(i => new OutputPadDefinition($"output{i}", typeof(T)))
            .ToArray();

        var factory = new RouterElementProcessorFactory<T>(outputCount, routeFunc);

        return new PipelineElementDefinition(name, inputs, outputs, factory, policy);
    }
}
