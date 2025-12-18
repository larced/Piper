namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Represents a connection (edge) between two pads in the pipeline graph.
/// At runtime, this materializes into a bounded Channel&lt;T&gt;.
/// </summary>
public interface IPipelineLinkDefinition
{
    /// <summary>
    /// Gets the output pad that produces data for this link.
    /// </summary>
    IOutputPadDefinition Source { get; }

    /// <summary>
    /// Gets the input pad that consumes data from this link.
    /// </summary>
    IInputPadDefinition Target { get; }

    /// <summary>
    /// Gets the policy that controls link behavior (e.g., buffer size).
    /// </summary>
    ILinkPolicy Policy { get; }
}
