using System.Net.Http.Json;
using System.Text.Json;

namespace JobTrackerApi.Tests;

// Tests for /stats (compares before and after, so other tests' jobs don't matter)
public class StatsEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;

    public StatsEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateOwnerClient();
    }

    [Fact]
    public async Task GetStats_CountsJobsResponsesAndXp()
    {
        var before = await GetStatsAsync();

        // Applied, they responded, then an interview: 15 + 25 + 50 XP
        var a = await CreateJobAsync("Stats A", JobStatus.Applied);
        await UpdateAsync(a, JobStatus.Applied, needsAction: true);
        await UpdateAsync(a, JobStatus.Interviewing);

        // Applied only: 15 XP
        await CreateJobAsync("Stats B", JobStatus.Applied);

        // Saved only: 5 XP
        await CreateJobAsync("Stats C", JobStatus.Saved);

        var after = await GetStatsAsync();

        Assert.Equal(3, after.TotalJobs - before.TotalJobs);
        Assert.Equal(2, after.Applications - before.Applications);
        Assert.Equal(1, after.Responses - before.Responses);
        Assert.Equal(110, after.Xp - before.Xp);
        Assert.Equal(1, after.ByStatus[JobStatus.Interviewing] - before.ByStatus[JobStatus.Interviewing]);
        Assert.Equal(1, after.ByStatus[JobStatus.Applied] - before.ByStatus[JobStatus.Applied]);
        Assert.Equal(1, after.ByStatus[JobStatus.Saved] - before.ByStatus[JobStatus.Saved]);
        Assert.Equal(Leveling.LevelFor(after.CollectedXp), after.Level);
    }

    [Fact]
    public async Task GetStats_StatusFlipFlopping_DoesNotFarmXp()
    {
        var before = await GetStatsAsync();

        var job = await CreateJobAsync("Flip Flop Co", JobStatus.Applied);
        await UpdateAsync(job, JobStatus.Interviewing);
        await UpdateAsync(job, JobStatus.Applied);
        await UpdateAsync(job, JobStatus.Interviewing);

        var after = await GetStatsAsync();

        // Applied 15 + Interviewing 50 + response 25, each counted once
        Assert.Equal(90, after.Xp - before.Xp);
    }

    [Fact]
    public async Task GetStats_CountsNeedsActionAndUpcomingInterviews()
    {
        var before = await GetStatsAsync();

        var job = await CreateJobAsync("Bell Co", JobStatus.Applied, needsAction: true);
        var interview = new CreateInterviewRequest(DateTimeOffset.UtcNow.AddDays(2), "Phone screen", null);
        var response = await _client.PostAsJsonAsync($"/jobs/{job.Id}/interviews", interview, JsonOptions);
        response.EnsureSuccessStatusCode();

        var after = await GetStatsAsync();

        Assert.Equal(1, after.NeedsAction - before.NeedsAction);
        Assert.Equal(1, after.UpcomingInterviews - before.UpcomingInterviews);
        Assert.InRange(after.ResponseRate, 0, 1);
    }

    [Fact]
    public async Task CollectXp_MovesPendingIntoCollected()
    {
        await CreateJobAsync("Tip Jar Co", JobStatus.Applied);
        var before = await GetStatsAsync();
        Assert.True(before.PendingXp > 0);

        var response = await _client.PostAsync("/stats/collect", null);
        response.EnsureSuccessStatusCode();
        var collect = await response.Content.ReadFromJsonAsync<CollectResponse>(JsonOptions);

        Assert.NotNull(collect);
        Assert.Equal(before.PendingXp, collect.Collected);
        Assert.Equal(before.Level, collect.PreviousLevel);
        Assert.Equal(0, collect.Stats.PendingXp);
        Assert.Equal(collect.Stats.Xp, collect.Stats.CollectedXp);
        Assert.Equal(Leveling.LevelFor(collect.Stats.CollectedXp), collect.Stats.Level);

        // Nothing new earned, nothing to collect
        var again = await _client.PostAsync("/stats/collect", null);
        var second = await again.Content.ReadFromJsonAsync<CollectResponse>(JsonOptions);
        Assert.Equal(0, second!.Collected);
    }

    // Helper: read /stats
    private async Task<StatsResponse> GetStatsAsync()
    {
        var stats = await _client.GetFromJsonAsync<StatsResponse>("/stats", JsonOptions);
        return stats!;
    }

    // Helper: add a job and return it
    private async Task<Job> CreateJobAsync(string company, JobStatus status, bool needsAction = false)
    {
        var request = new CreateJobRequest(company, "Software Engineer", "Irvine, CA",
            status, null, null, null, NeedsAction: needsAction);

        var response = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Job>(JsonOptions))!;
    }

    // Helper: change a job's status and flag
    private async Task UpdateAsync(Job job, JobStatus status, bool needsAction = false)
    {
        var request = new UpdateJobRequest(job.Company, job.Role, job.Location,
            status, job.DateApplied, job.Link, job.Notes, job.Deadline, needsAction);

        var response = await _client.PutAsJsonAsync($"/jobs/{job.Id}", request, JsonOptions);
        response.EnsureSuccessStatusCode();
    }
}
