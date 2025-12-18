using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Validation;

/// <summary>
/// Validates pipeline definitions for correctness (type compatibility,
/// required connections, no illegal fan-in, etc.).
/// </summary>
public interface IPipelineValidator
{
    /// <summary>
    /// Validates the given pipeline definition.
    /// Throws an exception if validation fails.
    /// </summary>
    /// <param name="definition">The pipeline definition to validate.</param>
    void Validate(IPipelineDefinition definition);
}
