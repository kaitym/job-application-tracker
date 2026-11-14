using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobTrackerApi.Tests;

// Tests for the /jobs endpoints
public class JobsEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    // Read and write enums as text, same as the API
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;

    public JobsEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateOwnerClient();
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

    [Fact]
    public async Task PostJob_WithDeadlineAndNeedsAction_SavesThem()
    {
        var request = new CreateJobRequest("Order Up Co", "Software Engineer", "Irvine, CA",
            JobStatus.Applied, new DateOnly(2026, 11, 1), null, null,
            Deadline: new DateOnly(2026, 12, 1), NeedsAction: true);

        var response = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);
        var job = await response.Content.ReadFromJsonAsync<Job>(JsonOptions);

        Assert.NotNull(job);
        Assert.Equal(new DateOnly(2026, 12, 1), job.Deadline);
        Assert.True(job.NeedsAction);
        Assert.NotNull(job.RespondedAt);
    }

    [Fact]
    public async Task PutJob_NeedsAction_KeepsFirstRespondedAt()
    {
        var created = await CreateJobAsync("Bell Ringer Inc");
        Assert.Null(created.RespondedAt);

        var first = await UpdateAsync(created, JobStatus.Applied, needsAction: true);
        Assert.NotNull(first.RespondedAt);

        var cleared = await UpdateAsync(created, JobStatus.Interviewing, needsAction: false);
        var again = await UpdateAsync(created, JobStatus.Interviewing, needsAction: true);

        Assert.False(cleared.NeedsAction);
        Assert.Equal(first.RespondedAt, cleared.RespondedAt);
        Assert.Equal(first.RespondedAt, again.RespondedAt);
    }

    [Fact]
    public async Task PostJob_RecordsFirstHistoryEntry()
    {
        var created = await CreateJobAsync("History Start Co", JobStatus.Saved);

        var history = await _client.GetFromJsonAsync<List<JobStatusChange>>(
            $"/jobs/{created.Id}/history", JsonOptions);

        Assert.NotNull(history);
        var entry = Assert.Single(history);
        Assert.Null(entry.FromStatus);
        Assert.Equal(JobStatus.Saved, entry.ToStatus);
    }

    [Fact]
    public async Task PutJob_StatusChange_AddsHistoryEntry()
    {
        var created = await CreateJobAsync("History Move Co", JobStatus.Saved);

        await UpdateAsync(created, JobStatus.Applied);
        await UpdateAsync(created, JobStatus.Applied); // same status, no new entry
        await UpdateAsync(created, JobStatus.Interviewing);

        var history = await _client.GetFromJsonAsync<List<JobStatusChange>>(
            $"/jobs/{created.Id}/history", JsonOptions);

        Assert.NotNull(history);
        Assert.Equal(3, history.Count);
        Assert.Equal(JobStatus.Saved, history[1].FromStatus);
        Assert.Equal(JobStatus.Applied, history[1].ToStatus);
        Assert.Equal(JobStatus.Applied, history[2].FromStatus);
        Assert.Equal(JobStatus.Interviewing, history[2].ToStatus);
    }

    [Fact]
    public async Task GetHistory_Missing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/jobs/99999/history");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_FilterByStatus_ReturnsOnlyThatStatus()
    {
        var offer = await CreateJobAsync("Filter Offer Co", JobStatus.Offer);
        await CreateJobAsync("Filter Saved Co", JobStatus.Saved);

        var jobs = await _client.GetFromJsonAsync<List<Job>>("/jobs?status=Offer", JsonOptions);

        Assert.NotNull(jobs);
        Assert.Contains(jobs, j => j.Id == offer.Id);
        Assert.All(jobs, j => Assert.Equal(JobStatus.Offer, j.Status));
    }

    [Fact]
    public async Task GetJobs_Search_IsCaseInsensitive()
    {
        var match = await CreateJobAsync("Espresso Labs");
        await CreateJobAsync("Matcha Works");

        var jobs = await _client.GetFromJsonAsync<List<Job>>("/jobs?search=espresso", JsonOptions);

        Assert.NotNull(jobs);
        Assert.Contains(jobs, j => j.Id == match.Id);
        Assert.DoesNotContain(jobs, j => j.Company == "Matcha Works");
    }

    [Fact]
    public async Task GetJobs_FilterByNeedsAction_ReturnsOnlyFlaggedJobs()
    {
        var flagged = await CreateJobAsync("Flagged Co", needsAction: true);

        var jobs = await _client.GetFromJsonAsync<List<Job>>("/jobs?needsAction=true", JsonOptions);

        Assert.NotNull(jobs);
        Assert.Contains(jobs, j => j.Id == flagged.Id);
        Assert.All(jobs, j => Assert.True(j.NeedsAction));
    }

    [Fact]
    public async Task GetGoneCold_ReturnsOnlyOldAppliedJobs()
    {
        var cold = await CreateJobAsync("Iced Over Inc");
        var coldButResponded = await CreateJobAsync("Iced But Replied Inc", needsAction: true);
        var oldSaved = await CreateJobAsync("Old Saved Inc", JobStatus.Saved);
        var fresh = await CreateJobAsync("Fresh Brew Inc");

        await BackdateHistoryAsync(cold.Id, days: 45);
        await BackdateHistoryAsync(coldButResponded.Id, days: 45);
        await BackdateHistoryAsync(oldSaved.Id, days: 45);

        var jobs = await _client.GetFromJsonAsync<List<Job>>("/jobs/gone-cold?days=30", JsonOptions);

        Assert.NotNull(jobs);
        Assert.Contains(jobs, j => j.Id == cold.Id);
        Assert.DoesNotContain(jobs, j => j.Id == coldButResponded.Id);
        Assert.DoesNotContain(jobs, j => j.Id == oldSaved.Id);
        Assert.DoesNotContain(jobs, j => j.Id == fresh.Id);
    }

    [Fact]
    public async Task GetGoneCold_InvalidDays_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/jobs/gone-cold?days=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Helper: add a job and return it
    private async Task<Job> CreateJobAsync(string company, JobStatus status = JobStatus.Applied, bool needsAction = false)
    {
        var request = new CreateJobRequest(company, "Software Engineer", "Irvine, CA",
            status, new DateOnly(2026, 11, 14), null, null, NeedsAction: needsAction);

        var response = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);
        response.EnsureSuccessStatusCode();

        var job = await response.Content.ReadFromJsonAsync<Job>(JsonOptions);
        return job!;
    }

    // Helper: change a job's status and flag, return the updated job
    private async Task<Job> UpdateAsync(Job job, JobStatus status, bool needsAction = false)
    {
        var request = new UpdateJobRequest(job.Company, job.Role, job.Location,
            status, job.DateApplied, job.Link, job.Notes, job.Deadline, needsAction);

        var response = await _client.PutAsJsonAsync($"/jobs/{job.Id}", request, JsonOptions);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<Job>(JsonOptions);
        return updated!;
    }

    // Helper: pretend a job's history happened days ago
    private async Task BackdateHistoryAsync(int jobId, int days)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var then = DateTime.UtcNow.AddDays(-days);
        await db.JobStatusChanges
            .Where(c => c.JobId == jobId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ChangedAt, then));
    }
}
