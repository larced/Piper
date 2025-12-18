using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Definition.Implementations;

/// <summary>
/// Default implementation of element policy.
/// </summary>
public class ElementPolicy : IElementPolicy
{
    public int DegreeOfParallelism { get; init; } = 1;

    public static ElementPolicy Default => new();
}

/// <summary>
/// Implementation of merge policy.
/// </summary>
public sealed class MergePolicy : ElementPolicy, IMergePolicy
{
    public MergeMode Mode { get; init; } = MergeMode.Interleave;

    public static new MergePolicy Default => new();
}

/// <summary>
/// Implementation of tee policy.
/// </summary>
public sealed class TeePolicy : ElementPolicy, ITeePolicy
{
    public TeeMode Mode { get; init; } = TeeMode.BlockAll;

    public static new TeePolicy Default => new();
}
