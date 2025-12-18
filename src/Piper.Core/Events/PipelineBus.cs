using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Piper.Core.Events;

/// <summary>
/// Implementation of pipeline event bus using channels.
/// </summary>
public sealed class PipelineBus : IPipelineBus, IDisposable
{
    private readonly Channel<IPipelineEvent> _channel;

    public PipelineBus(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _channel = Channel.CreateBounded<IPipelineEvent>(options);
    }

    public void Publish(IPipelineEvent evt)
    {
        if (evt == null)
            throw new ArgumentNullException(nameof(evt));

        // Use TryWrite to avoid blocking if buffer is full (drops oldest based on FullMode)
        _channel.Writer.TryWrite(evt);
    }

    public async IAsyncEnumerable<IPipelineEvent> Events([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var evt in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return evt;
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
    }
}
