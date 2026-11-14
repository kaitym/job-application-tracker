// What the client sends to schedule an interview
public record CreateInterviewRequest(
    DateTimeOffset ScheduledAt,
    string? Kind,
    string? Notes
);
