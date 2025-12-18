using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Builder;

/// <summary>
/// Factory for creating runtime instances from pipeline definitions.
/// </summary>
public interface IPipelineRuntimeFactory
{
    /// <summary>
    /// Creates a runtime instance from a pipeline definition.
    /// </summary>
    /// <param name="definition">The pipeline definition to materialize.</param>
    /// <returns>A runtime instance ready to be started.</returns>
    IPipelineRuntime Build(IPipelineDefinition definition);
}
