namespace JobTrackerApi.Tests;

// Unit tests for XP and levels (no database needed)
public class LevelingTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(249, 2)]
    [InlineData(250, 3)]
    [InlineData(5000, 10)]
    [InlineData(99999, 10)]
    public void LevelFor_UsesThresholds(int xp, int expectedLevel)
    {
        Assert.Equal(expectedLevel, Leveling.LevelFor(xp));
    }

    [Fact]
    public void XpForNextLevel_IsNextThreshold()
    {
        Assert.Equal(100, Leveling.XpForNextLevel(0));
        Assert.Equal(250, Leveling.XpForNextLevel(120));
    }

    [Fact]
    public void XpForNextLevel_AtMaxLevel_IsNull()
    {
        Assert.Null(Leveling.XpForNextLevel(5000));
    }

    [Fact]
    public void XpForReaching_RewardsBiggerStepsMore()
    {
        Assert.True(Leveling.XpForReaching(JobStatus.Offer) > Leveling.XpForReaching(JobStatus.Interviewing));
        Assert.True(Leveling.XpForReaching(JobStatus.Interviewing) > Leveling.XpForReaching(JobStatus.Applied));
        Assert.True(Leveling.XpForReaching(JobStatus.Applied) > Leveling.XpForReaching(JobStatus.Saved));
        Assert.Equal(0, Leveling.XpForReaching(JobStatus.Ghosted));
    }
}
