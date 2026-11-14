using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobTrackerApi.Tests;

// Tests for the /jobs endpoints
public class JobsEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    // Read and write enums as text, same as the API
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public JobsEndpointTests(ApiFactory factory)
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

    [Fact]
    public async Task GetJobById_Existing_ReturnsJob()
    {
        var created = await CreateJobAsync("Toshiba");

        var response = await _client.GetAsync($"/jobs/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var job = await response.Content.ReadFromJsonAsync<Job>(JsonOptions);
        Assert.Equal(created, job);
    }

    [Fact]
    public async Task GetJobById_Missing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/jobs/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutJob_Existing_UpdatesJob()
    {
        var created = await CreateJobAsync("Quartus");
        var update = new UpdateJobRequest("Quartus", "Software Engineer I", "Irvine, CA",
            JobStatus.Interviewing, new DateOnly(2026, 11, 10), null, "Phone screen Friday");

        var response = await _client.PutAsJsonAsync($"/jobs/{created.Id}", update, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var job = await _client.GetFromJsonAsync<Job>($"/jobs/{created.Id}", JsonOptions);
        Assert.NotNull(job);
        Assert.Equal(created.Id, job.Id);
        Assert.Equal("Software Engineer I", job.Role);
        Assert.Equal(JobStatus.Interviewing, job.Status);
        Assert.Equal("Phone screen Friday", job.Notes);
    }

    [Fact]
    public async Task PutJob_Missing_ReturnsNotFound()
    {
        var update = new UpdateJobRequest("Nobody", "Ghost Role", "Nowhere",
            JobStatus.Saved, null, null, null);

        var response = await _client.PutAsJsonAsync("/jobs/99999", update, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutJob_MissingRole_ReturnsBadRequest()
    {
        var created = await CreateJobAsync("Toshiba");
        var update = new UpdateJobRequest("Toshiba", "", "Irvine, CA",
            JobStatus.Applied, null, null, null);

        var response = await _client.PutAsJsonAsync($"/jobs/{created.Id}", update, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteJob_Existing_RemovesJob()
    {
        var created = await CreateJobAsync("Delete Me Inc");

        var response = await _client.DeleteAsync($"/jobs/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/jobs/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteJob_Missing_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/jobs/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Helper: add a job and return it
    private async Task<Job> CreateJobAsync(string company)
    {
        var request = new CreateJobRequest(company, "Software Engineer", "Irvine, CA",
            JobStatus.Applied, new DateOnly(2026, 11, 14), null, null);

        var response = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);
        response.EnsureSuccessStatusCode();

        var job = await response.Content.ReadFromJsonAsync<Job>(JsonOptions);
        return job!;
    }
}
