using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Elements;

/// <summary>
/// Factory for creating tee element processors.
/// </summary>
public sealed class TeeElementProcessorFactory<T> : IElementProcessorFactory
{
    private readonly int _outputCount;
    private readonly ITeePolicy _policy;

    public TeeElementProcessorFactory(int outputCount, ITeePolicy policy)
    {
        if (outputCount < 2)
            throw new ArgumentException("Tee element must have at least 2 outputs.", nameof(outputCount));

        _outputCount = outputCount;
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    public IElementProcessor CreateProcessor()
    {
        return new TeeElementProcessor<T>(_outputCount, _policy);
    }
}

/// <summary>
/// Processor for tee elements that duplicate items to multiple outputs.
/// </summary>
internal sealed class TeeElementProcessor<T> : IElementProcessor
{
    private readonly int _outputCount;
    private readonly ITeePolicy _policy;

    public TeeElementProcessor(int outputCount, ITeePolicy policy)
    {
        _outputCount = outputCount;
        _policy = policy;
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
                if (_policy.Mode == TeeMode.BlockAll)
                {
                    // Wait for all outputs to accept the item
                    var tasks = writers.Select(w => w.WriteAsync(item, cancellationToken).AsTask()).ToList();
                    await Task.WhenAll(tasks);
                }
                else if (_policy.Mode == TeeMode.DropSlow)
                {
                    // Try to write to all, but don't wait for slow ones
                    foreach (var writer in writers)
                    {
                        writer.TryWrite(item);
                    }
                }
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
/// Helper class for creating tee element definitions.
/// </summary>
public static class TeeElement
{
    public static IPipelineElementDefinition Create<T>(
        string name,
        int outputCount,
        ITeePolicy? policy = null)
    {
        if (outputCount < 2)
            throw new ArgumentException("Tee element must have at least 2 outputs.", nameof(outputCount));

        var inputs = new[] { new InputPadDefinition("input", typeof(T)) };
        var outputs = Enumerable.Range(0, outputCount)
            .Select(i => new OutputPadDefinition($"output{i}", typeof(T)))
            .ToArray();

        var teePolicy = policy ?? TeePolicy.Default;
        var factory = new TeeElementProcessorFactory<T>(outputCount, teePolicy);

        return new PipelineElementDefinition(name, inputs, outputs, factory, teePolicy);
    }
}
