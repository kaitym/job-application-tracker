namespace JobTrackerApi.Tests;

// Unit tests for reading careers links
public class BoardLinkTests
{
    [Theory]
    [InlineData("https://boards.greenhouse.io/pinterest", JobBoard.Greenhouse, "pinterest")]
    [InlineData("https://job-boards.greenhouse.io/twitch/jobs/123", JobBoard.Greenhouse, "twitch")]
    [InlineData("https://boards.greenhouse.io/embed/job_board?for=acme", JobBoard.Greenhouse, "acme")]
    [InlineData("https://jobs.lever.co/palantir/6ed76ce8", JobBoard.Lever, "palantir")]
    [InlineData("https://jobs.ashbyhq.com/notion", JobBoard.Ashby, "notion")]
    [InlineData("https://jobs.smartrecruiters.com/WesternDigital/744000161594186", JobBoard.SmartRecruiters, "WesternDigital")]
    public void KnownBoards_Parse(string url, JobBoard board, string slug)
    {
        Assert.True(BoardLink.TryParse(url, out var parsedBoard, out var parsedSlug));
        Assert.Equal(board, parsedBoard);
        Assert.Equal(slug, parsedSlug);
    }

    [Theory]
    [InlineData("https://www.linkedin.com/jobs/view/123")]
    [InlineData("https://intuit.wd1.myworkdayjobs.com/careers")]
    [InlineData("not a link")]
    [InlineData("https://boards.greenhouse.io/")]
    public void OtherLinks_DoNotParse(string url)
    {
        Assert.False(BoardLink.TryParse(url, out _, out _));
    }
}
