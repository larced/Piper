using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Definition.Implementations;

/// <summary>
/// Implementation of pipeline definition.
/// </summary>
public sealed class PipelineDefinition : IPipelineDefinition
{
    public string Name { get; }
    public IReadOnlyList<IPipelineElementDefinition> Elements { get; }
    public IReadOnlyList<IPipelineLinkDefinition> Links { get; }

    public PipelineDefinition(
        string name,
        IEnumerable<IPipelineElementDefinition> elements,
        IEnumerable<IPipelineLinkDefinition> links)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pipeline name cannot be null or whitespace.", nameof(name));

        Name = name;
        Elements = elements?.ToList() ?? throw new ArgumentNullException(nameof(elements));
        Links = links?.ToList() ?? throw new ArgumentNullException(nameof(links));
    }
}
