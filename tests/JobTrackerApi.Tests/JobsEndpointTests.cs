using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JobTrackerApi.Tests;

// Tests for the /jobs endpoints
public class JobsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    // Read and write enums as text, same as the API
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public JobsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetJobs_ReturnsOk()
    {
        var response = await _client.GetAsync("/jobs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var jobs = await response.Content.ReadFromJsonAsync<List<Job>>(JsonOptions);
        Assert.NotNull(jobs);
    }

    [Fact]
    public async Task PostJob_ReturnsCreatedJob()
    {
        var request = new CreateJobRequest("Toshiba", "Software Engineer", "Irvine, CA",
            JobStatus.Applied, new DateOnly(2026, 11, 14), null, null);

        var response = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var job = await response.Content.ReadFromJsonAsync<Job>(JsonOptions);
        Assert.NotNull(job);
        Assert.True(job.Id > 0);
        Assert.Equal("Toshiba", job.Company);
        Assert.Equal(JobStatus.Applied, job.Status);
        Assert.Equal($"/jobs/{job.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task PostJob_MissingCompany_ReturnsBadRequest()
    {
        var request = new CreateJobRequest("", "Software Engineer", "Irvine, CA",
            JobStatus.Saved, null, null, null);

        var response = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostJob_ThenGetJobs_IncludesNewJob()
    {
        var request = new CreateJobRequest("Quartus", "Junior Developer", "San Diego, CA",
            JobStatus.Saved, null, "https://example.com/job", "Referral from career fair");

        var postResponse = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);
        var created = await postResponse.Content.ReadFromJsonAsync<Job>(JsonOptions);

        var jobs = await _client.GetFromJsonAsync<List<Job>>("/jobs", JsonOptions);

        Assert.NotNull(jobs);
        Assert.Contains(jobs, j => j.Id == created!.Id && j.Company == "Quartus");
    }
}