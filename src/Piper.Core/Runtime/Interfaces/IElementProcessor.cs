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
/// 
/// COMPLETION OWNERSHIP:
/// - Elements are responsible for completing their output channels in finally blocks
/// - The runtime never directly completes channels; it only signals cancellation
/// - This ensures proper cleanup even when cancellation occurs
/// - Single-writer-per-output-pad is enforced by validation
/// </summary>
public interface IElementProcessor
{
    /// <summary>
    /// Executes the element's processing logic.
    /// Runs continuously until cancellation or completion.
    /// 
    /// IMPORTANT: Always complete output channels in a finally block to ensure
    /// proper cleanup even when cancelled. Example:
    /// <code>
    /// var writer = context.GetOutputWriter&lt;T&gt;("output");
    /// try
    /// {
    ///     // Process items
    /// }
    /// finally
    /// {
    ///     writer.Complete();
    /// }
    /// </code>
    /// </summary>
    /// <param name="context">Runtime context providing access to pads and the event bus.</param>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    Task RunAsync(IElementRuntimeContext context, CancellationToken cancellationToken);
}
