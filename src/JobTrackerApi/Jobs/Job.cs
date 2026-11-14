// Job model definition
public record Job(
    int Id,
    string Company,
    string Role,
    string Location,
    JobStatus Status,
    DateOnly? DateApplied, // ? implies that the field is optional, may be null
    string? Link,
    string? Notes,
    DateOnly? Deadline, // last day to apply
    bool NeedsAction, // they responded, my move ("Order up!")
    DateTime? RespondedAt, // first time they responded, set by the API
    string? Description = null // the posting's description
    );
