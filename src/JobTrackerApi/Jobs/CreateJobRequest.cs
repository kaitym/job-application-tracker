// What the client sends to the server to create a new job (No Id, the API will generate one)

public record CreateJobRequest(
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
