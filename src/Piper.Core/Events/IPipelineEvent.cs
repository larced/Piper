namespace Piper.Core.Events;

/// <summary>
/// Base interface for all pipeline events.
/// </summary>
public interface IPipelineEvent
{
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the name of the element associated with this event, if any.
    /// </summary>
    string? ElementName { get; }
}

/// <summary>
/// Represents an error event in the pipeline.
/// </summary>
public interface IPipelineErrorEvent : IPipelineEvent
{
    /// <summary>
    /// Gets the exception that caused the error.
    /// </summary>
    Exception Exception { get; }
}
