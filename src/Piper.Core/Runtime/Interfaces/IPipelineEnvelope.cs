namespace Piper.Core.Runtime.Interfaces;

/// <summary>
/// Wraps a payload with context and error information for uniform error handling.
/// Optional but recommended for structured error propagation.
/// </summary>
/// <typeparam name="T">The type of the payload.</typeparam>
public interface IPipelineEnvelope<out T>
{
    /// <summary>
    /// Gets the actual data payload.
    /// </summary>
    T Payload { get; }

    /// <summary>
    /// Gets the pipeline context for this item.
    /// </summary>
    IPipelineContext Context { get; }

    /// <summary>
    /// Gets a value indicating whether this envelope represents a faulted item.
    /// </summary>
    bool IsFaulted { get; }

    /// <summary>
    /// Gets the error associated with this envelope, if any.
    /// </summary>
    Exception? Error { get; }
}
