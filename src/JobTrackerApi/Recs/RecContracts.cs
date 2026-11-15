// Request and response shapes for the bulletin board

// Add a company by pasting its careers link, or by board + id
public record AddCompanyRequest(string? Url, JobBoard? Board, string? Slug, string? Name);

public record RecBoardResponse(List<JobRec> Recs, int NewCount, DateTime? LastRefreshedAt, List<WatchedCompany> Companies);

public record RefreshResult(bool Ran, int Checked, int Added, List<string> Errors);
