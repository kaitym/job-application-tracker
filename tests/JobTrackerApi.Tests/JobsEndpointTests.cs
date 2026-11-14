using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JobTrackerApi.Tests;

// Tests for the /jobs endpoints
public class JobsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public JobsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetJobs_ReturnsOkAndEmptyList()
    {
        var response = await _client.GetAsync("/jobs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var jobs = await response.Content.ReadFromJsonAsync<List<Job>>();
        Assert.NotNull(jobs);
        Assert.Empty(jobs);
    }
}
