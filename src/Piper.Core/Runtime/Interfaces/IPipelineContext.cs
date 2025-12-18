namespace Piper.Core.Runtime.Interfaces;

/// <summary>
/// Provides shared context for pipeline execution, including correlation IDs
/// and custom metadata.
/// </summary>
public interface IPipelineContext
{
    /// <summary>
    /// Gets the correlation ID for tracking related items through the pipeline.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// Gets custom metadata items associated with this context.
    /// </summary>
    IReadOnlyDictionary<string, object?> Items { get; }
}
