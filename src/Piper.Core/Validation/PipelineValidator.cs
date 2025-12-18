using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Validation;

/// <summary>
/// Implementation of pipeline validator.
/// </summary>
public sealed class PipelineValidator : IPipelineValidator
{
    public void Validate(IPipelineDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        ValidateUniqueElementNames(definition);
        ValidateRequiredPadsConnected(definition);
        ValidateTypeCompatibility(definition);
        ValidateNoIllegalFanIn(definition);
    }

    private static void ValidateUniqueElementNames(IPipelineDefinition definition)
    {
        var duplicates = definition.Elements
            .GroupBy(e => e.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            throw new InvalidOperationException(
                $"Pipeline contains duplicate element names: {string.Join(", ", duplicates)}");
        }
    }

    private static void ValidateRequiredPadsConnected(IPipelineDefinition definition)
    {
        foreach (var element in definition.Elements)
        {
            // Check required inputs
            foreach (var input in element.Inputs.Where(i => i.IsRequired))
            {
                if (!definition.Links.Any(l => l.Target == input))
                {
                    throw new InvalidOperationException(
                        $"Required input pad '{input.Name}' on element '{element.Name}' is not connected.");
                }
            }

            // Check required outputs
            foreach (var output in element.Outputs.Where(o => o.IsRequired))
            {
                if (!definition.Links.Any(l => l.Source == output))
                {
                    throw new InvalidOperationException(
                        $"Required output pad '{output.Name}' on element '{element.Name}' is not connected.");
                }
            }
        }
    }

    private static void ValidateTypeCompatibility(IPipelineDefinition definition)
    {
        foreach (var link in definition.Links)
        {
            if (link.Source.DataType != link.Target.DataType)
            {
                throw new InvalidOperationException(
                    $"Type mismatch in link: source pad '{link.Source.Name}' has type {link.Source.DataType.Name}, " +
                    $"but target pad '{link.Target.Name}' has type {link.Target.DataType.Name}.");
            }
        }
    }

    private static void ValidateNoIllegalFanIn(IPipelineDefinition definition)
    {
        var inputPadConnections = definition.Links
            .GroupBy(l => l.Target)
            .Where(g => g.Count() > 1)
            .ToList();

        if (inputPadConnections.Any())
        {
            var violations = inputPadConnections
                .Select(g => $"'{g.Key.Name}' has {g.Count()} incoming connections")
                .ToList();

            throw new InvalidOperationException(
                $"Illegal fan-in detected (multiple links to same input pad): {string.Join(", ", violations)}. " +
                "Use a Merge element to combine multiple sources.");
        }
    }
}
