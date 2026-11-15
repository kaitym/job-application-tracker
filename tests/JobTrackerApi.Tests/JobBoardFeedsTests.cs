using System.Text.Json;

namespace JobTrackerApi.Tests;

// Unit tests for reading each job board's JSON
public class JobBoardFeedsTests
{
    [Fact]
    public void Greenhouse_ParsesJobs()
    {
        using var json = JsonDocument.Parse("""
            { "jobs": [ { "id": 8268331, "title": "Software Engineer, New Grad", "absolute_url": "https://example.com/1",
              "location": { "name": "San Francisco, CA, US" }, "updated_at": "2026-11-06T16:55:13-05:00",
              "first_published": "2026-10-29T18:46:31-04:00" } ] }
            """);

        var posting = Assert.Single(JobBoardFeeds.ParseGreenhouse(json));

        Assert.Equal("8268331", posting.ExternalId);
        Assert.Equal("Software Engineer, New Grad", posting.Title);
        Assert.Equal("San Francisco, CA, US", posting.Location);
        Assert.Equal(new DateTime(2026, 10, 29, 22, 46, 31, DateTimeKind.Utc), posting.PostedAt);
    }

    [Fact]
    public void Lever_ParsesAllLocationsAndMillisecondDates()
    {
        using var json = JsonDocument.Parse("""
            [ { "id": "abc", "text": "Junior Developer", "hostedUrl": "https://jobs.lever.co/acme/abc", "createdAt": 1786469891368,
                "categories": { "location": "Irvine, CA", "allLocations": ["Irvine, CA", "Remote, US"] } } ]
            """);

        var posting = Assert.Single(JobBoardFeeds.ParseLever(json));

        Assert.Equal("abc", posting.ExternalId);
        Assert.Equal("Irvine, CA; Remote, US", posting.Location);
        Assert.NotNull(posting.PostedAt);
    }

    [Fact]
    public void Ashby_MarksRemoteJobs()
    {
        using var json = JsonDocument.Parse("""
            { "jobs": [ { "id": "x1", "title": "Software Engineer, Early Career", "location": "United States",
              "jobUrl": "https://jobs.ashbyhq.com/acme/x1", "publishedAt": "2026-11-01T00:00:00Z", "isRemote": true } ] }
            """);

        var posting = Assert.Single(JobBoardFeeds.ParseAshby(json));

        Assert.Equal("United States (Remote)", posting.Location);
    }

    [Fact]
    public void SmartRecruiters_BuildsPostingLinks()
    {
        using var json = JsonDocument.Parse("""
            { "content": [ { "id": "744", "name": "Software Engineer, Early Career", "releasedDate": "2026-11-13T23:46:34.464Z",
              "location": { "city": "Irvine", "region": "CA", "fullLocation": "Irvine, CA, United States", "remote": false } } ] }
            """);

        var posting = Assert.Single(JobBoardFeeds.ParseSmartRecruiters(json, "WesternDigital"));

        Assert.Equal("https://jobs.smartrecruiters.com/WesternDigital/744", posting.Url);
        Assert.Equal("Irvine, CA, United States", posting.Location);
    }
}
