using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobTrackerApi.Tests;

// Tests for the interview endpoints
public class InterviewEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;

    public InterviewEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostInterview_ReturnsCreatedInterviewInUtc()
    {
        var job = await CreateJobAsync("Phone Screen Co");
        var request = new CreateInterviewRequest(
            new DateTimeOffset(2026, 12, 2, 10, 0, 0, TimeSpan.FromHours(-8)), "Phone screen", null);

        var response = await _client.PostAsJsonAsync($"/jobs/{job.Id}/interviews", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var interview = await response.Content.ReadFromJsonAsync<Interview>(JsonOptions);
        Assert.NotNull(interview);
        Assert.Equal(job.Id, interview.JobId);
        Assert.Equal(new DateTime(2026, 12, 2, 18, 0, 0, DateTimeKind.Utc), interview.ScheduledAt);
        Assert.Equal("Phone screen", interview.Kind);
    }

    [Fact]
    public async Task PostInterview_MissingJob_ReturnsNotFound()
    {
        var request = new CreateInterviewRequest(DateTimeOffset.UtcNow.AddDays(3), "Technical", null);

        var response = await _client.PostAsJsonAsync("/jobs/99999/interviews", request, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostInterview_MissingTime_ReturnsBadRequest()
    {
        var job = await CreateJobAsync("No Time Co");

        var response = await _client.PostAsJsonAsync($"/jobs/{job.Id}/interviews",
            new { kind = "Technical" }, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInterviews_ReturnsSoonestFirst()
    {
        var job = await CreateJobAsync("Two Rounds Co");
        await ScheduleAsync(job.Id, DateTimeOffset.UtcNow.AddDays(10), "Onsite");
        await ScheduleAsync(job.Id, DateTimeOffset.UtcNow.AddDays(2), "Phone screen");

        var interviews = await _client.GetFromJsonAsync<List<Interview>>(
            $"/jobs/{job.Id}/interviews", JsonOptions);

        Assert.NotNull(interviews);
        Assert.Equal(["Phone screen", "Onsite"], interviews.Select(i => i.Kind));
    }

    [Fact]
    public async Task GetInterviews_MissingJob_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/jobs/99999/interviews");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteInterview_RemovesIt()
    {
        var job = await CreateJobAsync("Cancelled Co");
        var interview = await ScheduleAsync(job.Id, DateTimeOffset.UtcNow.AddDays(4), "Technical");

        var response = await _client.DeleteAsync($"/jobs/{job.Id}/interviews/{interview.Id}");
        var again = await _client.DeleteAsync($"/jobs/{job.Id}/interviews/{interview.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
    }

    [Fact]
    public async Task GetUpcoming_IncludesSoonInterviewsWithCompany()
    {
        var job = await CreateJobAsync("Upcoming Brew Co");
        var soon = await ScheduleAsync(job.Id, DateTimeOffset.UtcNow.AddDays(3), "Technical");
        var later = await ScheduleAsync(job.Id, DateTimeOffset.UtcNow.AddDays(40), "Onsite");
        var past = await ScheduleAsync(job.Id, DateTimeOffset.UtcNow.AddDays(-3), "Phone screen");

        var upcoming = await _client.GetFromJsonAsync<List<UpcomingInterview>>(
            "/interviews/upcoming?days=14", JsonOptions);

        Assert.NotNull(upcoming);
        var match = Assert.Single(upcoming, u => u.Id == soon.Id);
        Assert.Equal("Upcoming Brew Co", match.Company);
        Assert.DoesNotContain(upcoming, u => u.Id == later.Id);
        Assert.DoesNotContain(upcoming, u => u.Id == past.Id);
    }

    [Fact]
    public async Task DeleteJob_AlsoDeletesItsInterviews()
    {
        var job = await CreateJobAsync("Gone Soon Co");
        var interview = await ScheduleAsync(job.Id, DateTimeOffset.UtcNow.AddDays(2), "Technical");

        await _client.DeleteAsync($"/jobs/{job.Id}");

        var upcoming = await _client.GetFromJsonAsync<List<UpcomingInterview>>(
            "/interviews/upcoming", JsonOptions);
        Assert.NotNull(upcoming);
        Assert.DoesNotContain(upcoming, u => u.Id == interview.Id);
    }

    // Helper: add a job and return it
    private async Task<Job> CreateJobAsync(string company)
    {
        var request = new CreateJobRequest(company, "Software Engineer", "Irvine, CA",
            JobStatus.Interviewing, new DateOnly(2026, 11, 14), null, null);

        var response = await _client.PostAsJsonAsync("/jobs", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Job>(JsonOptions))!;
    }

    // Helper: schedule an interview and return it
    private async Task<Interview> ScheduleAsync(int jobId, DateTimeOffset when, string kind)
    {
        var request = new CreateInterviewRequest(when, kind, null);

        var response = await _client.PostAsJsonAsync($"/jobs/{jobId}/interviews", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Interview>(JsonOptions))!;
    }
}
