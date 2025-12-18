using System.Threading.Channels;
using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Definition.Implementations;

/// <summary>
/// Default implementation of link policy.
/// </summary>
public sealed class LinkPolicy : ILinkPolicy
{
    public int BufferSize { get; init; } = 50;
    public BoundedChannelFullMode FullMode { get; init; } = BoundedChannelFullMode.Wait;

    public static LinkPolicy Default => new();
}
