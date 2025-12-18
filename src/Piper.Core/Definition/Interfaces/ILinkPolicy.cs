using System.Threading.Channels;

namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Policy that controls link-level behavior, particularly buffering.
/// </summary>
public interface ILinkPolicy
{
    /// <summary>
    /// Gets the buffer size for the channel created for this link.
    /// Default is 50.
    /// </summary>
    int BufferSize { get; }

    /// <summary>
    /// Gets the behavior when the buffer is full.
    /// </summary>
    BoundedChannelFullMode FullMode { get; }
}
