using Piper.Core.Definition.Implementations;
using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Builder;

/// <summary>
/// Implementation of pipeline builder.
/// </summary>
public sealed class PipelineBuilder : IPipelineBuilder
{
    private readonly string _pipelineName;
    private readonly List<IPipelineElementDefinition> _elements = new();
    private readonly List<IPipelineLinkDefinition> _links = new();
    private readonly Dictionary<string, IElementPolicy> _elementPolicies = new();
    private readonly Dictionary<(IOutputPadDefinition, IInputPadDefinition), ILinkPolicy> _linkPolicies = new();

    public PipelineBuilder(string pipelineName)
    {
        if (string.IsNullOrWhiteSpace(pipelineName))
            throw new ArgumentException("Pipeline name cannot be null or whitespace.", nameof(pipelineName));

        _pipelineName = pipelineName;
    }

    public IPipelineBuilder Add(IPipelineElementDefinition element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        _elements.Add(element);
        return this;
    }

    public IPipelineBuilder Link(IOutputPadDefinition from, IInputPadDefinition to)
    {
        if (from == null)
            throw new ArgumentNullException(nameof(from));
        if (to == null)
            throw new ArgumentNullException(nameof(to));

        var policy = _linkPolicies.TryGetValue((from, to), out var p) ? p : null;
        _links.Add(new PipelineLinkDefinition(from, to, policy));
        return this;
    }

    public IPipelineBuilder ConfigureElement(string elementName, Func<IElementPolicy, IElementPolicy> update)
    {
        if (string.IsNullOrWhiteSpace(elementName))
            throw new ArgumentException("Element name cannot be null or whitespace.", nameof(elementName));
        if (update == null)
            throw new ArgumentNullException(nameof(update));

        var element = _elements.FirstOrDefault(e => e.Name == elementName);
        if (element == null)
            throw new InvalidOperationException($"Element '{elementName}' not found.");

        _elementPolicies[elementName] = update(element.Policy);
        return this;
    }

    public IPipelineBuilder ConfigureLink(IOutputPadDefinition from, IInputPadDefinition to, Func<ILinkPolicy, ILinkPolicy> update)
    {
        if (from == null)
            throw new ArgumentNullException(nameof(from));
        if (to == null)
            throw new ArgumentNullException(nameof(to));
        if (update == null)
            throw new ArgumentNullException(nameof(update));

        var existingLink = _links.FirstOrDefault(l => l.Source == from && l.Target == to);
        var currentPolicy = existingLink?.Policy ?? LinkPolicy.Default;
        
        _linkPolicies[(from, to)] = update(currentPolicy);
        return this;
    }

    public IPipelineDefinition BuildDefinition()
    {
        // Apply configured policies to elements
        var finalElements = _elements.Select(e =>
        {
            if (_elementPolicies.TryGetValue(e.Name, out var policy))
            {
                return new PipelineElementDefinition(e.Name, e.Inputs, e.Outputs, e.ProcessorFactory, policy);
            }
            return e;
        }).ToList();

        // Apply configured policies to links
        var finalLinks = _links.Select(l =>
        {
            if (_linkPolicies.TryGetValue((l.Source, l.Target), out var policy))
            {
                return new PipelineLinkDefinition(l.Source, l.Target, policy);
            }
            return l;
        }).ToList();

        return new PipelineDefinition(_pipelineName, finalElements, finalLinks);
    }
}
