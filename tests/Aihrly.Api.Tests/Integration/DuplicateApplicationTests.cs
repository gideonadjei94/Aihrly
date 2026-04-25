using Xunit;
using System.Net;
using Aihrly.Api.Tests.Fixtures;
using Aihrly.Api.Tests.Helpers;

namespace Aihrly.Api.Tests.Integration;

public class DuplicateApplicationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiClient _client = new(factory.CreateClient());

    [Fact]
    public async Task Apply_WithSameEmailAndSameJob_Returns409()
    {
        var jobResponse = await _client.PostAsync("/api/jobs", new
        {
            title = "Backend Engineer",
            description = "Build APIs",
            location = "Accra, Ghana"
        });

        var jobId = await _client.ReadCreatedIdAsync(jobResponse);

        var applicationBody = new
        {
            candidateName  = "Kofi Adu",
            candidateEmail = "kofi.adu@email.com"
        };

        // Act — apply once (should succeed)
        var firstResponse = await _client.PostAsync($"/api/jobs/{jobId}/applications", applicationBody);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act — apply again with the same email (should be rejected)
        var secondResponse = await _client.PostAsync($"/api/jobs/{jobId}/applications", applicationBody);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Apply_WithSameEmailButDifferentJob_Returns201()
    {
        // Same candidate can apply to different jobs — this should succeed
        var job1Response = await _client.PostAsync("/api/jobs", new
        {
            title = "Frontend Engineer", description = "Build UIs", location = "Remote"
        });
        var job2Response = await _client.PostAsync("/api/jobs", new
        {
            title = "DevOps Engineer", description = "Build pipelines", location = "Remote"
        });

        var job1Id = await _client.ReadCreatedIdAsync(job1Response);
        var job2Id = await _client.ReadCreatedIdAsync(job2Response);

        var body = new { candidateName = "Ama Asante", candidateEmail = "ama@email.com" };

        var response1 = await _client.PostAsync($"/api/jobs/{job1Id}/applications", body);
        var response2 = await _client.PostAsync($"/api/jobs/{job2Id}/applications", body);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    }
}
