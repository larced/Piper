using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Represents the definition of a single element (node) in a pipeline.
/// An element has typed input/output pads, a policy, and a factory
/// for creating runtime processor instances.
/// </summary>
public interface IPipelineElementDefinition
{
    /// <summary>
    /// Gets the unique name of this element within the pipeline.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the collection of input pads for this element.
    /// </summary>
    IReadOnlyList<IInputPadDefinition> Inputs { get; }

    /// <summary>
    /// Gets the collection of output pads for this element.
    /// </summary>
    IReadOnlyList<IOutputPadDefinition> Outputs { get; }

    /// <summary>
    /// Gets the policy that controls element behavior (e.g., parallelism).
    /// </summary>
    IElementPolicy Policy { get; }

    /// <summary>
    /// Gets the factory that creates runtime processor instances for this element.
    /// </summary>
    IElementProcessorFactory ProcessorFactory { get; }
}
