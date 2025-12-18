namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Policy that controls element-level behavior.
/// </summary>
public interface IElementPolicy
{
    /// <summary>
    /// Gets the degree of parallelism for this element (number of concurrent workers).
    /// Default is 1.
    /// NOTE: In the current version, only DegreeOfParallelism = 1 is supported.
    /// Multi-processor parallelism is reserved for future implementation.
    /// </summary>
    int DegreeOfParallelism { get; }
}
