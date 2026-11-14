// An upcoming interview with its job's details
public record UpcomingInterview(
    int Id,
    int JobId,
    string Company,
    string Role,
    DateTime ScheduledAt,
    string? Kind,
    string? Notes
);
