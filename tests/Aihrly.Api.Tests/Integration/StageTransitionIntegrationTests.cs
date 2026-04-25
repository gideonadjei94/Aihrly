using Xunit;
using System.Net;
using Aihrly.Api.Data;
using Aihrly.Api.Tests.Fixtures;
using Aihrly.Api.Tests.Helpers;

namespace Aihrly.Api.Tests.Integration;

public class StageTransitionIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiClient _client = new(factory.CreateClient());

    [Fact]
    public async Task MoveStage_ValidTransition_Returns204()
    {
        var applicationId = await CreateApplicationAsync();

        var response = await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Screening" },
            teamMemberId: AppDbContext.AliceId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task MoveStage_SkippingStages_Returns400WithClearMessage()
    {
        var applicationId = await CreateApplicationAsync();

        // Applied → Hired is illegal (skipping Screening, Interview, Offer)
        var response = await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Hired" },
            teamMemberId: AppDbContext.AliceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Applied", body);
        Assert.Contains("Hired", body);
    }

    [Fact]
    public async Task MoveStage_MissingHeader_Returns400()
    {
        var applicationId = await CreateApplicationAsync();

        // No X-Team-Member-Id header
        var response = await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Screening" },
            teamMemberId: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MoveStage_FromTerminalStage_Returns400()
    {
        var applicationId = await CreateApplicationAsync();

        // Reject first — puts it in a terminal stage
        await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Rejected" },
            teamMemberId: AppDbContext.AliceId);

        // Try to move again from Rejected
        var response = await _client.PatchAsync(
            $"/api/applications/{applicationId}/stage",
            new { stage = "Screening" },
            teamMemberId: AppDbContext.AliceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("terminal", body);
    }

    private async Task<Guid> CreateApplicationAsync()
    {
        var jobResponse = await _client.PostAsync("/api/jobs", new
        {
            title = "Software Engineer", description = "Write code", location = "Tema"
        });
        var jobId = await _client.ReadCreatedIdAsync(jobResponse);

        var appResponse = await _client.PostAsync($"/api/jobs/{jobId}/applications", new
        {
            candidateName  = "Nana Akua",
            candidateEmail = $"nana.{Guid.NewGuid()}@email.com"
        });
        return await _client.ReadCreatedIdAsync(appResponse);
    }
}
