using System.Net.Http.Json;
using System.Text.Json;

namespace Aihrly.Api.Tests.Helpers;

// Wrapper around HttpClient — keeps test code readable and removes repetition
public class ApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body, Guid? teamMemberId = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };

        if (teamMemberId.HasValue)
            request.Headers.Add("X-Team-Member-Id", teamMemberId.Value.ToString());

        return await http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PatchAsync<T>(string url, T body, Guid? teamMemberId = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(body)
        };

        if (teamMemberId.HasValue)
            request.Headers.Add("X-Team-Member-Id", teamMemberId.Value.ToString());

        return await http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T body, Guid? teamMemberId = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(body)
        };

        if (teamMemberId.HasValue)
            request.Headers.Add("X-Team-Member-Id", teamMemberId.Value.ToString());

        return await http.SendAsync(request);
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    // Parses just the "id" from a 201 Created response body
    public async Task<Guid> ReadCreatedIdAsync(HttpResponseMessage response)
    {
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("id").GetGuid();
    }
}
