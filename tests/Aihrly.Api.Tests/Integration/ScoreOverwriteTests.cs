using Xunit;
using System.Net;
using System.Text.Json;
using Aihrly.Api.Data;
using Aihrly.Api.Tests.Fixtures;
using Aihrly.Api.Tests.Helpers;

namespace Aihrly.Api.Tests.Integration;

public class ScoreOverwriteTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiClient _client = new(factory.CreateClient());

    [Fact]
    public async Task SubmitScore_Twice_SecondValueWins()
    {
        // Arrange
        var applicationId = await CreateApplicationAsync();

        // Alice submits a culture-fit score of 2
        var firstResponse = await _client.PutAsync(
            $"/api/applications/{applicationId}/scores/culture-fit",
            new { score = 2, comment = "Needs improvement." },
            teamMemberId: AppDbContext.AliceId);

        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        // Kwame submits again with a score of 5
        var secondResponse = await _client.PutAsync(
            $"/api/applications/{applicationId}/scores/culture-fit",
            new { score = 5, comment = "Outstanding culture fit." },
            teamMemberId: AppDbContext.KwameId);

        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

        // Act — read the full profile to inspect the score
        var profile = await _client.GetAsync<JsonElement>($"/api/applications/{applicationId}");

        // Assert — second submission wins on value, scorer, and comment
        var scores = profile.GetProperty("scores").EnumerateArray().ToList();
        var cultureFit = scores.Single(s => s.GetProperty("dimension").GetString() == "CultureFit");

        Assert.Equal(5, cultureFit.GetProperty("score").GetInt32());
        Assert.Equal("Outstanding culture fit.", cultureFit.GetProperty("comment").GetString());
        Assert.Equal("Kwame Boateng", cultureFit.GetProperty("scoredBy").GetString());
    }

    [Fact]
    public async Task SubmitScore_ThreeDifferentDimensions_AllStoredIndependently()
    {
        // Proves that scoring one dimension doesn't affect the others
        var applicationId = await CreateApplicationAsync();

        await _client.PutAsync($"/api/applications/{applicationId}/scores/culture-fit",
            new { score = 4, comment = "Good fit." }, AppDbContext.AliceId);

        await _client.PutAsync($"/api/applications/{applicationId}/scores/interview",
            new { score = 3, comment = "Average performance." }, AppDbContext.KwameId);

        await _client.PutAsync($"/api/applications/{applicationId}/scores/assessment",
            new { score = 5, comment = "Excellent test results." }, AppDbContext.SaraId);

        var profile = await _client.GetAsync<JsonElement>($"/api/applications/{applicationId}");
        var scores = profile.GetProperty("scores").EnumerateArray().ToList();

        Assert.Equal(3, scores.Count);
        Assert.Equal(4, scores.Single(s => s.GetProperty("dimension").GetString() == "CultureFit").GetProperty("score").GetInt32());
        Assert.Equal(3, scores.Single(s => s.GetProperty("dimension").GetString() == "Interview").GetProperty("score").GetInt32());
        Assert.Equal(5, scores.Single(s => s.GetProperty("dimension").GetString() == "Assessment").GetProperty("score").GetInt32());
    }

    private async Task<Guid> CreateApplicationAsync()
    {
        var jobResponse = await _client.PostAsync("/api/jobs", new
        {
            title = "Data Analyst", description = "Analyse data", location = "Kumasi"
        });
        var jobId = await _client.ReadCreatedIdAsync(jobResponse);

        var appResponse = await _client.PostAsync($"/api/jobs/{jobId}/applications", new
        {
            candidateName  = "Abena Mensah",
            candidateEmail = $"abena.{Guid.NewGuid()}@email.com"
        });
        return await _client.ReadCreatedIdAsync(appResponse);
    }
}
