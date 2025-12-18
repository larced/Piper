namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Represents the complete static definition of a pipeline,
/// including all elements, links, and validation metadata.
/// </summary>
public interface IPipelineDefinition
{
    /// <summary>
    /// Gets the name of the pipeline.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the collection of all elements in the pipeline.
    /// </summary>
    IReadOnlyList<IPipelineElementDefinition> Elements { get; }

    /// <summary>
    /// Gets the collection of all links between elements in the pipeline.
    /// </summary>
    IReadOnlyList<IPipelineLinkDefinition> Links { get; }
}
