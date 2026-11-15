namespace JobTrackerApi.Tests;

// Unit tests for which postings get pinned
public class RecMatcherTests
{
    [Theory]
    [InlineData("Software Engineer, New Grad")]
    [InlineData("University Grad Software Engineer 2027")]
    [InlineData("Software Engineer I")]
    [InlineData("Software Development Engineer, Early Career")]
    [InlineData("Associate Software Engineer")]
    [InlineData("Junior Backend Developer")]
    [InlineData("Software Engineer 1, New College Grad")]
    public void EarlyCareerSoftwareRoles_Match(string title)
    {
        Assert.True(RecMatcher.IsEarlyCareerSoftwareRole(title));
    }

    [Theory]
    [InlineData("Senior Software Engineer")]
    [InlineData("Staff Engineer, New Grad Programs")]
    [InlineData("Software Engineer II")]
    [InlineData("Software Engineering Intern")]
    [InlineData("Engineering Manager")]
    [InlineData("B2B Growth Marketing Manager")]
    [InlineData("Software Engineer")] // no early-career signal
    [InlineData("Associate Product Marketing Manager")]
    public void OtherRoles_DoNotMatch(string title)
    {
        Assert.False(RecMatcher.IsEarlyCareerSoftwareRole(title));
    }

    [Theory]
    [InlineData("Irvine, CA", 3)]
    [InlineData("Costa Mesa, California", 3)]
    [InlineData("San Diego, CA, US", 2)]
    [InlineData("Carlsbad, CA", 2)]
    [InlineData("San Francisco, CA, US; Remote, US", 1)]
    [InlineData("Remote", 1)]
    [InlineData("Remote, United States", 1)]
    [InlineData("Remote, Australia", 0)]
    [InlineData("Seattle, WA", 0)]
    [InlineData("London, UK", 0)]
    public void Locations_GetPriority(string location, int expected)
    {
        Assert.Equal(expected, RecMatcher.LocationPriority(location));
    }

    [Fact]
    public void Score_NeedsBothRoleAndPlace()
    {
        Assert.Equal(3, RecMatcher.Score("Software Engineer, New Grad", "Irvine, CA"));
        Assert.Equal(0, RecMatcher.Score("Software Engineer, New Grad", "Seattle, WA"));
        Assert.Equal(0, RecMatcher.Score("Senior Software Engineer", "Irvine, CA"));
    }

    [Theory]
    [InlineData("Pinterest", "University Grad Software Engineer 2027 (USA)", "Pinterest", "University Grad Software Engineer 2027", true)]
    [InlineData("Twitch", "Software Engineer I - Community Growth", "Twitch", "Software Engineer I, Community Growth", true)]
    [InlineData("Twitch", "Software Engineer I - Payments", "Twitch", "Software Engineer I, Community Growth", false)]
    [InlineData("Twitch", "Software Engineer I, Community Growth", "Pinterest", "Software Engineer I, Community Growth", false)]
    public void IsSameJob_MatchesSameCompanyAndRole(string recCompany, string recTitle, string jobCompany, string jobRole, bool expected)
    {
        Assert.Equal(expected, RecMatcher.IsSameJob(recCompany, recTitle, jobCompany, jobRole));
    }
}
