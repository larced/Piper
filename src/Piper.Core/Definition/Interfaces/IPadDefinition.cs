namespace Piper.Core.Definition.Interfaces;

/// <summary>
/// Base interface for typed connection points (pads) on elements.
/// Pads are declared up-front and validated for type compatibility.
/// </summary>
public interface IPadDefinition
{
    /// <summary>
    /// Gets the name of this pad within its element.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the data type that flows through this pad.
    /// </summary>
    Type DataType { get; }

    /// <summary>
    /// Gets a value indicating whether this pad must be connected for validation to pass.
    /// </summary>
    bool IsRequired { get; }
}

/// <summary>
/// Represents an input pad (data consumer) on an element.
/// </summary>
public interface IInputPadDefinition : IPadDefinition
{
}

/// <summary>
/// Represents an output pad (data producer) on an element.
/// </summary>
public interface IOutputPadDefinition : IPadDefinition
{
}
