// Job search numbers plus coffee shop progress
public record StatsResponse(
    int TotalJobs,
    Dictionary<JobStatus, int> ByStatus,
    int Applications, // jobs that got past Saved
    int Responses, // jobs where they responded
    double ResponseRate, // responses / applications
    int NeedsAction,
    int UpcomingInterviews,
    int Xp,
    int Level,
    int? NextLevelXp // null at max level
);
