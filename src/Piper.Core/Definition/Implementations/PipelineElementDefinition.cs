using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Definition.Implementations;

/// <summary>
/// Implementation of pipeline element definition.
/// </summary>
public sealed class PipelineElementDefinition : IPipelineElementDefinition
{
    public string Name { get; }
    public IReadOnlyList<IInputPadDefinition> Inputs { get; }
    public IReadOnlyList<IOutputPadDefinition> Outputs { get; }
    public IElementPolicy Policy { get; }
    public IElementProcessorFactory ProcessorFactory { get; }

    public PipelineElementDefinition(
        string name,
        IEnumerable<IInputPadDefinition> inputs,
        IEnumerable<IOutputPadDefinition> outputs,
        IElementProcessorFactory processorFactory,
        IElementPolicy? policy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Element name cannot be null or whitespace.", nameof(name));

        Name = name;
        Inputs = inputs?.ToList() ?? new List<IInputPadDefinition>();
        Outputs = outputs?.ToList() ?? new List<IOutputPadDefinition>();
        ProcessorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
        Policy = policy ?? ElementPolicy.Default;
    }
}
