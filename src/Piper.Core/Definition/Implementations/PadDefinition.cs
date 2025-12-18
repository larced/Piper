using Piper.Core.Definition.Interfaces;

namespace Piper.Core.Definition.Implementations;

/// <summary>
/// Base implementation of pad definition.
/// </summary>
public abstract class PadDefinition : IPadDefinition
{
    public string Name { get; }
    public Type DataType { get; }
    public bool IsRequired { get; }

    protected PadDefinition(string name, Type dataType, bool isRequired = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pad name cannot be null or whitespace.", nameof(name));

        Name = name;
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        IsRequired = isRequired;
    }
}

/// <summary>
/// Implementation of input pad definition.
/// </summary>
public sealed class InputPadDefinition : PadDefinition, IInputPadDefinition
{
    public InputPadDefinition(string name, Type dataType, bool isRequired = true)
        : base(name, dataType, isRequired)
    {
    }
}

/// <summary>
/// Implementation of output pad definition.
/// </summary>
public sealed class OutputPadDefinition : PadDefinition, IOutputPadDefinition
{
    public OutputPadDefinition(string name, Type dataType, bool isRequired = true)
        : base(name, dataType, isRequired)
    {
    }
}
