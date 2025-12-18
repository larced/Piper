using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Definition.Implementations;

/// <summary>
/// Implementation of pipeline link definition.
/// </summary>
public sealed class PipelineLinkDefinition : IPipelineLinkDefinition
{
    public IOutputPadDefinition Source { get; }
    public IInputPadDefinition Target { get; }
    public ILinkPolicy Policy { get; }

    public PipelineLinkDefinition(
        IOutputPadDefinition source,
        IInputPadDefinition target,
        ILinkPolicy? policy = null)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Policy = policy ?? LinkPolicy.Default;
    }
}
