using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Validation;

/// <summary>
/// Implementation of pipeline inspector.
/// </summary>
public sealed class PipelineInspector : IPipelineInspector
{
    public PipelineDescription Describe(IPipelineDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        return new PipelineDescription(
            definition.Name,
            definition.Elements.Count,
            definition.Links.Count);
    }
}
