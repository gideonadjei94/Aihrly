using Aihrly.Api.Data;
using Aihrly.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Infrastructure;


public class NotificationWorker(
    INotificationQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification worker started.");

        await foreach (var message in queue.ReadAllAsync(stoppingToken))
        {
            await ProcessAsync(message);
        }

        logger.LogInformation("Notification worker stopped.");
    }

    private async Task ProcessAsync(NotificationMessage message)
    {
        try
        {
            
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var notification = new Notification
            {
                Id            = Guid.NewGuid(),
                ApplicationId = message.ApplicationId,
                Type          = message.Type,
                SentAt        = DateTime.UtcNow
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync();

            // Simulating sending an email 
            logger.LogInformation(
                "Notification sent: application {ApplicationId} moved to '{Type}'.",
                message.ApplicationId, message.Type);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to process notification for application {ApplicationId}.",
                message.ApplicationId);
        }
    }
}
