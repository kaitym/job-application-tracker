// XP and levels for the coffee shop
public static class Leveling
{
    // XP when they respond (Order up!)
    public const int ResponseXp = 25;

    // XP needed to reach each level (index 0 = level 1)
    public static readonly int[] LevelThresholds = [0, 100, 250, 500, 900, 1400, 2000, 2800, 3800, 5000];

    // XP the first time a job reaches a status
    public static int XpForReaching(JobStatus status)
    {
        return status switch
        {
            JobStatus.Saved => 5,
            JobStatus.Applied => 15,
            JobStatus.Interviewing => 50,
            JobStatus.Offer => 200,
            JobStatus.Rejected => 5,
            JobStatus.Ghosted => 0,
            _ => 0
        };
    }

    // Level for a total amount of XP
    public static int LevelFor(int xp)
    {
        return LevelThresholds.Count(threshold => xp >= threshold);
    }

    // XP needed for the next level, or null at max level
    public static int? XpForNextLevel(int xp)
    {
        var level = LevelFor(xp);
        return level < LevelThresholds.Length ? LevelThresholds[level] : null;
    }
}
