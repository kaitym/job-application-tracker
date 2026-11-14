// Job model definition
public record Job(
    int Id,
    string Company, 
    string Role, 
    string Location, 
    JobStatus Status,
    DateOnly? DateApplied, // ? implies that the field is optional, may be null
    string? Link, 
    string? Notes
    );

