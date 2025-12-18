namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Policy for merge elements that combine multiple input streams.
/// </summary>
public interface IMergePolicy : IElementPolicy
{
    /// <summary>
    /// Gets the mode that determines how inputs are combined.
    /// </summary>
    MergeMode Mode { get; }
}

/// <summary>
/// Defines how a merge element combines multiple input streams.
/// </summary>
public enum MergeMode
{
    /// <summary>
    /// Interleave items from all inputs as they arrive.
    /// </summary>
    Interleave,

    /// <summary>
    /// Zip inputs together, waiting for one item from each before proceeding.
    /// </summary>
    Zip,

    /// <summary>
    /// Prioritize inputs by index, reading from higher-priority inputs first.
    /// </summary>
    Priority
}
