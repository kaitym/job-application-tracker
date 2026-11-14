// A scheduled interview for a job
public record Interview(
    int Id,
    int JobId,
    DateTime ScheduledAt, // stored in UTC
    string? Kind, // phone screen, technical, onsite...
    string? Notes
);
