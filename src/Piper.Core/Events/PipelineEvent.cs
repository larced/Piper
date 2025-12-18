namespace Piper.Core.Events;

/// <summary>
/// Base implementation of pipeline event.
/// </summary>
public class PipelineEvent : IPipelineEvent
{
    public DateTimeOffset Timestamp { get; }
    public string? ElementName { get; }

    public PipelineEvent(string? elementName = null)
    {
        Timestamp = DateTimeOffset.UtcNow;
        ElementName = elementName;
    }
}

/// <summary>
/// Implementation of pipeline error event.
/// </summary>
public sealed class PipelineErrorEvent : PipelineEvent, IPipelineErrorEvent
{
    public Exception Exception { get; }

    public PipelineErrorEvent(Exception exception, string? elementName = null)
        : base(elementName)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}

/// <summary>
/// Event published when the pipeline state changes.
/// </summary>
public sealed class PipelineStateChangedEvent : PipelineEvent
{
    public Runtime.Interfaces.PipelineState OldState { get; }
    public Runtime.Interfaces.PipelineState NewState { get; }

    public PipelineStateChangedEvent(
        Runtime.Interfaces.PipelineState oldState,
        Runtime.Interfaces.PipelineState newState)
        : base(null)
    {
        OldState = oldState;
        NewState = newState;
    }
}

/// <summary>
/// Event published when an element starts processing.
/// </summary>
public sealed class ElementStartedEvent : PipelineEvent
{
    public ElementStartedEvent(string elementName)
        : base(elementName)
    {
    }
}

/// <summary>
/// Event published when an element completes processing.
/// </summary>
public sealed class ElementCompletedEvent : PipelineEvent
{
    public ElementCompletedEvent(string elementName)
        : base(elementName)
    {
    }
}

/// <summary>
/// Event published when an element faults.
/// </summary>
public sealed class ElementFaultedEvent : PipelineEvent
{
    public Exception Exception { get; }

    public ElementFaultedEvent(string elementName, Exception exception)
        : base(elementName)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}
