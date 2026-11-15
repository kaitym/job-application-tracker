// A company whose job board gets checked for recs
public record WatchedCompany(
    int Id,
    string Name,
    JobBoard Board,
    string Slug, // the company's id on that board, e.g. "pinterest"
    DateTime CreatedAt,
    DateTime? LastCheckedAt,
    string? LastError
);
