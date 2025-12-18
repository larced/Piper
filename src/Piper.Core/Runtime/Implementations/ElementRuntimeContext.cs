using System.Threading.Channels;
using Piper.Core.Events;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Runtime.Implementations;

/// <summary>
/// Implementation of element runtime context.
/// Provides access to typed channel endpoints for pads.
/// </summary>
public sealed class ElementRuntimeContext : IElementRuntimeContext
{
    private readonly Dictionary<string, object> _inputReaders = new();
    private readonly Dictionary<string, object> _outputWriters = new();

    public IPipelineBus Bus { get; }
    public IPipelineContext SharedContext { get; }

    public ElementRuntimeContext(IPipelineBus bus, IPipelineContext sharedContext)
    {
        Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        SharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
    }

    public void RegisterInputReader<T>(string padName, ChannelReader<T> reader)
    {
        if (string.IsNullOrWhiteSpace(padName))
            throw new ArgumentException("Pad name cannot be null or whitespace.", nameof(padName));
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        _inputReaders[padName] = reader;
    }

    public void RegisterOutputWriter<T>(string padName, ChannelWriter<T> writer)
    {
        if (string.IsNullOrWhiteSpace(padName))
            throw new ArgumentException("Pad name cannot be null or whitespace.", nameof(padName));
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        _outputWriters[padName] = writer;
    }

    public ChannelReader<T> GetInputReader<T>(string inputPadName)
    {
        if (!_inputReaders.TryGetValue(inputPadName, out var reader))
            throw new InvalidOperationException($"No input reader registered for pad '{inputPadName}'.");

        if (reader is not ChannelReader<T> typedReader)
            throw new InvalidOperationException($"Input reader for pad '{inputPadName}' is not of type ChannelReader<{typeof(T).Name}>.");

        return typedReader;
    }

    public ChannelWriter<T> GetOutputWriter<T>(string outputPadName)
    {
        if (!_outputWriters.TryGetValue(outputPadName, out var writer))
            throw new InvalidOperationException($"No output writer registered for pad '{outputPadName}'.");

        if (writer is not ChannelWriter<T> typedWriter)
            throw new InvalidOperationException($"Output writer for pad '{outputPadName}' is not of type ChannelWriter<{typeof(T).Name}>.");

        return typedWriter;
    }
}
