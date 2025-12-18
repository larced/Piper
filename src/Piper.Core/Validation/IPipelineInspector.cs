using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Validation;

/// <summary>
/// Provides introspection and description capabilities for pipeline definitions.
/// </summary>
public interface IPipelineInspector
{
    /// <summary>
    /// Generates a description of the pipeline for inspection purposes.
    /// </summary>
    /// <param name="definition">The pipeline definition to describe.</param>
    /// <returns>A description of the pipeline.</returns>
    PipelineDescription Describe(IPipelineDefinition definition);
}

/// <summary>
/// Describes a pipeline's structure for introspection.
/// </summary>
/// <param name="Name">The name of the pipeline.</param>
/// <param name="ElementCount">The number of elements in the pipeline.</param>
/// <param name="LinkCount">The number of links in the pipeline.</param>
public sealed record PipelineDescription(string Name, int ElementCount, int LinkCount);
