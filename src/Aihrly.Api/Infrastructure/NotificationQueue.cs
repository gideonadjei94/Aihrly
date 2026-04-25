using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Aihrly.Api.Infrastructure;

// In-memory queue backed by System.Threading.Channels
// Registered as a singleton so the same channel instance is shared across the app
public class NotificationQueue : INotificationQueue
{
    private readonly Channel<NotificationMessage> _channel =
        Channel.CreateUnbounded<NotificationMessage>(new UnboundedChannelOptions
        {
            SingleReader = true  
        });

    public void Enqueue(NotificationMessage message) =>
        _channel.Writer.TryWrite(message);

    public async IAsyncEnumerable<NotificationMessage> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
            yield return message;
    }
}
