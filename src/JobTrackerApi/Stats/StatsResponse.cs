// Job search numbers plus coffee shop progress
public record StatsResponse(
    int TotalJobs,
    Dictionary<JobStatus, int> ByStatus,
    int Applications, // jobs that got past Saved
    int Responses, // jobs where they responded
    double ResponseRate, // responses / applications
    int NeedsAction,
    int UpcomingInterviews,
    int Xp, // all XP earned so far
    int CollectedXp, // XP I've collected (sets the level)
    int PendingXp, // earned but not collected yet
    int Level,
    int LevelStartXp, // XP where this level began
    int? NextLevelXp // null at max level
);
