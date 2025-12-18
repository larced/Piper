using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Runtime.Implementations;

/// <summary>
/// Implementation of pipeline context.
/// </summary>
public sealed class PipelineContext : IPipelineContext
{
    public Guid CorrelationId { get; }
    public IReadOnlyDictionary<string, object?> Items { get; }

    public PipelineContext(Guid? correlationId = null, IReadOnlyDictionary<string, object?>? items = null)
    {
        CorrelationId = correlationId ?? Guid.NewGuid();
        Items = items ?? new Dictionary<string, object?>();
    }
}
