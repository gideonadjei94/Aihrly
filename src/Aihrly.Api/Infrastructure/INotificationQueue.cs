using Aihrly.Api.Entities;

namespace Aihrly.Api.Infrastructure;

public interface INotificationQueue
{
    void Enqueue(NotificationMessage message);
    IAsyncEnumerable<NotificationMessage> ReadAllAsync(CancellationToken cancellationToken);
}
