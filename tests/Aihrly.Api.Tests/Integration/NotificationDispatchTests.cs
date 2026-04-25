using Xunit;
using System.Net;
using Aihrly.Api.Data;
using Aihrly.Api.Tests.Fixtures;
using Aihrly.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Tests.Integration;


public class NotificationDispatchTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiClient _client = new(factory.CreateClient());

    [Fact]
    public async Task MoveToHired_PatchReturnsImmediately_AndNotificationIsWrittenToDb()
    {
        // Arrange — walk an application through the full pipeline to reach Offer
        var applicationId = await CreateApplicationAtOfferStageAsync();

        // Act — move to Hired
        var response = await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Hired", reason = "Best candidate." },
            teamMemberId: AppDbContext.AliceId);

        // The endpoint returns 204 immediately — not after the notification is sent
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // The worker runs asynchronously, so we give it a moment to write to the DB
        // In production this would be an event-driven assertion, but a short wait
        // is the standard pattern for testing background workers in integration tests
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert — notification row was written to the DB
        using var db = factory.CreateDbContext();
        var notification = await db.Notifications
            .SingleOrDefaultAsync(n => n.ApplicationId == applicationId && n.Type == "hired");

        Assert.NotNull(notification);
        Assert.Equal("hired", notification.Type);
    }

    [Fact]
    public async Task MoveToRejected_NotificationIsWritten_WithCorrectType()
    {
        var applicationId = await CreateApplicationAsync();

        // Move Applied → Rejected in one step (valid transition)
        await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Rejected", reason = "Not a fit." },
            teamMemberId: AppDbContext.KwameId);

        await Task.Delay(TimeSpan.FromSeconds(2));

        using var db = factory.CreateDbContext();
        var notification = await db.Notifications
            .SingleOrDefaultAsync(n => n.ApplicationId == applicationId && n.Type == "rejected");

        Assert.NotNull(notification);
    }

    [Fact]
    public async Task MoveToNonTerminalStage_NoNotificationIsDispatched()
    {
        // Moving to Screening should NOT trigger a notification
        var applicationId = await CreateApplicationAsync();

        await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Screening" },
            teamMemberId: AppDbContext.AliceId);

        await Task.Delay(TimeSpan.FromSeconds(2));

        using var db = factory.CreateDbContext();
        var count = await db.Notifications.CountAsync(n => n.ApplicationId == applicationId);

        Assert.Equal(0, count);
    }

    // Helper — creates a job, applies, and walks through Applied → Screening → Interview → Offer
    private async Task<Guid> CreateApplicationAtOfferStageAsync()
    {
        var applicationId = await CreateApplicationAsync();

        foreach (var stage in new[] { "Screening", "Interview", "Offer" })
        {
            await _client.PatchAsync(
                $"/api/applications/{applicationId}/stage",
                new { stage },
                teamMemberId: AppDbContext.AliceId);
        }

        return applicationId;
    }

    private async Task<Guid> CreateApplicationAsync()
    {
        var jobResponse = await _client.PostAsync("/api/jobs", new
        {
            title = "Product Manager",
            description = "Define the product",
            location = "Accra"
        });
        var jobId = await _client.ReadCreatedIdAsync(jobResponse);

        var appResponse = await _client.PostAsync($"/api/jobs/{jobId}/applications", new
        {
            candidateName  = "Kwesi Mensah",
            candidateEmail = $"kwesi.{Guid.NewGuid()}@email.com"
        });
        return await _client.ReadCreatedIdAsync(appResponse);
    }
}
