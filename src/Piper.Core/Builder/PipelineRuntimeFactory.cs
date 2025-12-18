using Piper.Core.Definition.Interfaces;
using Piper.Core.Runtime.Implementations;
using Piper.Core.Runtime.Interfaces;
using Piper.Core.Validation;

namespace Piper.Core.Builder;

/// <summary>
/// Implementation of pipeline runtime factory.
/// </summary>
public sealed class PipelineRuntimeFactory : IPipelineRuntimeFactory
{
    private readonly IPipelineValidator _validator;

    public PipelineRuntimeFactory(IPipelineValidator? validator = null)
    {
        _validator = validator ?? new PipelineValidator();
    }

    public IPipelineRuntime Build(IPipelineDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        // Validate before building runtime
        _validator.Validate(definition);

        return new PipelineRuntime(definition);
    }
}
