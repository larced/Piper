using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Builder;

/// <summary>
/// Builder for constructing pipeline definitions in a fluent, type-safe manner.
/// </summary>
public interface IPipelineBuilder
{
    /// <summary>
    /// Adds an element to the pipeline.
    /// </summary>
    /// <param name="element">The element definition to add.</param>
    /// <returns>The builder for chaining.</returns>
    IPipelineBuilder Add(IPipelineElementDefinition element);

    /// <summary>
    /// Creates a link between an output pad and an input pad.
    /// </summary>
    /// <param name="from">The source output pad.</param>
    /// <param name="to">The target input pad.</param>
    /// <returns>The builder for chaining.</returns>
    IPipelineBuilder Link(IOutputPadDefinition from, IInputPadDefinition to);

    /// <summary>
    /// Configures the policy for an element.
    /// </summary>
    /// <param name="elementName">The name of the element to configure.</param>
    /// <param name="update">A function to update the policy.</param>
    /// <returns>The builder for chaining.</returns>
    IPipelineBuilder ConfigureElement(string elementName, Func<IElementPolicy, IElementPolicy> update);

    /// <summary>
    /// Configures the policy for a link.
    /// </summary>
    /// <param name="from">The source output pad of the link.</param>
    /// <param name="to">The target input pad of the link.</param>
    /// <param name="update">A function to update the policy.</param>
    /// <returns>The builder for chaining.</returns>
    IPipelineBuilder ConfigureLink(IOutputPadDefinition from, IInputPadDefinition to, Func<ILinkPolicy, ILinkPolicy> update);

    /// <summary>
    /// Builds the pipeline definition.
    /// </summary>
    /// <returns>The constructed pipeline definition.</returns>
    IPipelineDefinition BuildDefinition();
}
