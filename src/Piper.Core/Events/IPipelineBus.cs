namespace Piper.Core.Events;

/// <summary>
/// Event bus for publishing and subscribing to pipeline events.
/// Provides observability into pipeline execution.
/// </summary>
public interface IPipelineBus
{
    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    /// <param name="evt">The event to publish.</param>
    void Publish(IPipelineEvent evt);

    /// <summary>
    /// Returns an async stream of all events.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the stream.</param>
    IAsyncEnumerable<IPipelineEvent> Events(CancellationToken cancellationToken);
}
