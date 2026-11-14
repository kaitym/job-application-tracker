// What the client sends to update a job (Id comes from the URL)
public record UpdateJobRequest(
    string Company,
    string Role,
    string Location,
    JobStatus Status,
    DateOnly? DateApplied,
    string? Link,
    string? Notes,
    DateOnly? Deadline = null,
    bool NeedsAction = false
);
