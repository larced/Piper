namespace Piper.Core.Runtime.Interfaces;

/// <summary>
/// Factory for creating element processor instances.
/// </summary>
public interface IElementProcessorFactory
{
    /// <summary>
    /// Creates a new processor instance for an element.
    /// </summary>
    IElementProcessor CreateProcessor();
}

/// <summary>
/// Represents the runtime behavior of an element.
/// Implementations define how items flow through the element.
/// </summary>
public interface IElementProcessor
{
    /// <summary>
    /// Executes the element's processing logic.
    /// Runs continuously until cancellation or completion.
    /// </summary>
    /// <param name="context">Runtime context providing access to pads and the event bus.</param>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    Task RunAsync(IElementRuntimeContext context, CancellationToken cancellationToken);
}
