using Xunit;
using System.Net.Http.Json;
using System.Text.Json;
using Aihrly.Api.Data;
using Aihrly.Api.Tests.Fixtures;
using Aihrly.Api.Tests.Helpers;

namespace Aihrly.Api.Tests.Integration;

public class NoteAuthorResolutionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiClient _client = new(factory.CreateClient());

    [Fact]
    public async Task AddNote_ThenListNotes_AuthorNameIsResolved_NotJustId()
    {
        // Arrange — create job and application
        var jobId = await CreateJobAsync();
        var applicationId = await CreateApplicationAsync(jobId);

        // Alice adds a note — her ID comes from the header, not the body
        await _client.PostAsync(
            $"/api/applications/{applicationId}/notes",
            new { type = "General", description = "Strong communication skills." },
            teamMemberId: AppDbContext.AliceId);

        // Act — list notes for the application
        var notes = await _client.GetAsync<JsonElement>($"/api/applications/{applicationId}/notes");

        
        var firstNote = notes.EnumerateArray().First();
        var authorName = firstNote.GetProperty("authorName").GetString();

        Assert.Equal("Alice Mensah", authorName);
    }

    [Fact]
    public async Task AddNote_CreatedByComesFromHeader_NotRequestBody()
    {
        var jobId = await CreateJobAsync();
        var applicationId = await CreateApplicationAsync(jobId);

        await _client.PostAsync(
            $"/api/applications/{applicationId}/notes",
            new { type = "Interview", description = "Good problem-solving ability." },
            teamMemberId: AppDbContext.KwameId);

        var notes = await _client.GetAsync<JsonElement>($"/api/applications/{applicationId}/notes");
        var authorName = notes.EnumerateArray().First().GetProperty("authorName").GetString();

        Assert.Equal("Kwame Boateng", authorName);
    }

    private async Task<Guid> CreateJobAsync()
    {
        var response = await _client.PostAsync("/api/jobs", new
        {
            title = "QA Engineer", description = "Test everything", location = "Accra"
        });
        return await _client.ReadCreatedIdAsync(response);
    }

    private async Task<Guid> CreateApplicationAsync(Guid jobId)
    {
        var response = await _client.PostAsync($"/api/jobs/{jobId}/applications", new
        {
            candidateName  = "Yaw Darko",
            candidateEmail = $"yaw.darko.{Guid.NewGuid()}@email.com"
        });
        return await _client.ReadCreatedIdAsync(response);
    }
}
