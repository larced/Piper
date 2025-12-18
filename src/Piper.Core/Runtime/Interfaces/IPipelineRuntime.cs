using Piper.Core.Events;

namespace Piper.Core.Runtime.Interfaces;

/// <summary>
/// Represents the runtime execution surface of a pipeline.
/// Manages lifecycle, state transitions, and provides access to events.
/// </summary>
public interface IPipelineRuntime : IAsyncDisposable
{
    /// <summary>
    /// Gets the current state of the pipeline.
    /// </summary>
    PipelineState State { get; }

    /// <summary>
    /// Gets the event bus for publishing and subscribing to pipeline events.
    /// </summary>
    IPipelineBus Bus { get; }

    /// <summary>
    /// Starts the pipeline, transitioning from Created/Prepared to Running.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Initiates graceful draining of the pipeline, allowing in-flight items to complete.
    /// </summary>
    Task DrainAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the pipeline immediately or after draining.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Represents the lifecycle state of a pipeline.
/// </summary>
public enum PipelineState
{
    /// <summary>
    /// Pipeline has been created but not prepared for execution.
    /// </summary>
    Created,

    /// <summary>
    /// Pipeline has been validated and prepared (channels created, bindings established).
    /// </summary>
    Prepared,

    /// <summary>
    /// Pipeline is actively processing items.
    /// </summary>
    Running,

    /// <summary>
    /// Pipeline is draining - no new items accepted, existing items being processed.
    /// </summary>
    Draining,

    /// <summary>
    /// Pipeline has stopped cleanly.
    /// </summary>
    Stopped,

    /// <summary>
    /// Pipeline has faulted due to an unrecoverable error.
    /// </summary>
    Faulted
}
