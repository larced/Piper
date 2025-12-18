using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Runtime.Implementations;

/// <summary>
/// Implementation of pipeline envelope.
/// </summary>
public sealed class PipelineEnvelope<T> : IPipelineEnvelope<T>
{
    public T Payload { get; }
    public IPipelineContext Context { get; }
    public bool IsFaulted { get; }
    public Exception? Error { get; }

    public PipelineEnvelope(T payload, IPipelineContext context, Exception? error = null)
    {
        Payload = payload;
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Error = error;
        IsFaulted = error != null;
    }

    public static PipelineEnvelope<T> Success(T payload, IPipelineContext? context = null)
    {
        return new PipelineEnvelope<T>(payload, context ?? new PipelineContext());
    }

    public static PipelineEnvelope<T> Failure(T payload, Exception error, IPipelineContext? context = null)
    {
        return new PipelineEnvelope<T>(payload, context ?? new PipelineContext(), error);
    }
}
