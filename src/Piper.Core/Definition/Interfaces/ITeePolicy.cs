namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Policy for tee elements that duplicate items to multiple outputs.
/// </summary>
public interface ITeePolicy : IElementPolicy
{
    /// <summary>
    /// Gets the mode that determines how the tee handles slow consumers.
    /// </summary>
    TeeMode Mode { get; }
}

/// <summary>
/// Defines how a tee element handles backpressure from multiple outputs.
/// </summary>
public enum TeeMode
{
    /// <summary>
    /// Block on all outputs - wait for all consumers to accept the item.
    /// </summary>
    BlockAll,

    /// <summary>
    /// Drop items for slow consumers rather than blocking.
    /// </summary>
    DropSlow
}
