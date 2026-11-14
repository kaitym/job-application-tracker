// What could be read from a job posting page (anything missing is null)
public record JobLinkPreview(
    string Link,
    string? Company,
    string? Role,
    string? Location,
    string? Description
);
