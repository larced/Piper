using System.Threading.Channels;
using Piper.Core.Events;

namespace Piper.Core.Runtime.Interfaces;

/// <summary>
/// Provides runtime context for an element processor, including access to
/// typed channel endpoints and shared pipeline context.
/// </summary>
public interface IElementRuntimeContext
{
    /// <summary>
    /// Gets the event bus for publishing pipeline events.
    /// </summary>
    IPipelineBus Bus { get; }

    /// <summary>
    /// Gets the shared pipeline context for correlation and metadata.
    /// </summary>
    IPipelineContext SharedContext { get; }

    /// <summary>
    /// Gets a typed reader for an input pad.
    /// </summary>
    /// <typeparam name="T">The data type of the pad.</typeparam>
    /// <param name="inputPadName">The name of the input pad.</param>
    /// <returns>A channel reader for consuming items.</returns>
    ChannelReader<T> GetInputReader<T>(string inputPadName);

    /// <summary>
    /// Gets a typed writer for an output pad.
    /// </summary>
    /// <typeparam name="T">The data type of the pad.</typeparam>
    /// <param name="outputPadName">The name of the output pad.</param>
    /// <returns>A channel writer for producing items.</returns>
    ChannelWriter<T> GetOutputWriter<T>(string outputPadName);
}
