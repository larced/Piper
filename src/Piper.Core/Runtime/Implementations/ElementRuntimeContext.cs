using System.Collections.Concurrent;
using System.Threading.Channels;
using Piper.Core.Events;
using Piper.Core.Runtime.Interfaces;

namespace Piper.Core.Runtime.Implementations;

/// <summary>
/// Implementation of element runtime context.
/// Provides access to typed channel endpoints for pads.
/// Optimized to avoid dictionary lookups and type checks in the hot path.
/// </summary>
public sealed class ElementRuntimeContext : IElementRuntimeContext
{
    // Store readers and writers with type information embedded in the key to avoid casting
    // Using ConcurrentDictionary for thread-safe access patterns
    private readonly ConcurrentDictionary<(string padName, Type dataType), object> _inputReaders = new();
    private readonly ConcurrentDictionary<(string padName, Type dataType), object> _outputWriters = new();

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

        // Store with type information in the key, avoiding later casts
        _inputReaders[(padName, typeof(T))] = reader;
    }

    public void RegisterOutputWriter<T>(string padName, ChannelWriter<T> writer)
    {
        if (string.IsNullOrWhiteSpace(padName))
            throw new ArgumentException("Pad name cannot be null or whitespace.", nameof(padName));
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        // Store with type information in the key, avoiding later casts
        _outputWriters[(padName, typeof(T))] = writer;
    }

    public ChannelReader<T> GetInputReader<T>(string inputPadName)
    {
        // Direct lookup with type - no casting needed, optimized for hot path
        var key = (inputPadName, typeof(T));
        if (_inputReaders.TryGetValue(key, out var reader))
        {
            // Safe cast - type is guaranteed by registration
            return (ChannelReader<T>)reader;
        }

        throw new InvalidOperationException(
            $"No input reader registered for pad '{inputPadName}' with type {typeof(T).Name}.");
    }

    public ChannelWriter<T> GetOutputWriter<T>(string outputPadName)
    {
        // Direct lookup with type - no casting needed, optimized for hot path
        var key = (outputPadName, typeof(T));
        if (_outputWriters.TryGetValue(key, out var writer))
        {
            // Safe cast - type is guaranteed by registration
            return (ChannelWriter<T>)writer;
        }

        throw new InvalidOperationException(
            $"No output writer registered for pad '{outputPadName}' with type {typeof(T).Name}.");
    }
}
