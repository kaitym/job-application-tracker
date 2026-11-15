using Microsoft.EntityFrameworkCore;

// Bulletin board: checks watched companies' job boards and pins matching roles
public class RecService(AppDbContext db, IJobBoardFetcher fetcher, JobService jobs, ILogger<RecService> logger)
{
    public static readonly TimeSpan StaleAfter = TimeSpan.FromHours(12);
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(60);

    // New recs, best first
    public async Task<RecBoardResponse> GetBoardAsync()
    {
        var menu = await db.Jobs.Select(j => new { j.Company, j.Role }).ToListAsync();
        var recs = (await db.JobRecs
            .Where(r => r.State == RecState.New)
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.PostedAt ?? r.FoundAt)
            .Take(80)
            .ToListAsync())
            // Skip anything already on the menu
            .Where(r => !menu.Any(j => RecMatcher.IsSameJob(r.Company, r.Title, j.Company, j.Role)))
            .Take(60)
            .ToList();

        var companies = await db.WatchedCompanies.OrderBy(c => c.Name).ToListAsync();
        var lastRefreshed = companies.Count == 0 ? null : companies.Min(c => c.LastCheckedAt);

        return new RecBoardResponse(recs, recs.Count, lastRefreshed, companies);
    }

    // Check every watched company (or only if it's been a while)
    public async Task<RefreshResult> RefreshAsync(bool onlyIfStale, CancellationToken cancellationToken)
    {
        var companies = await db.WatchedCompanies.ToListAsync(cancellationToken);
        var cutoff = DateTime.UtcNow - StaleAfter;

        if (onlyIfStale && companies.All(c => c.LastCheckedAt is not null && c.LastCheckedAt > cutoff))
        {
            return new RefreshResult(false, 0, 0, []);
        }

        var added = 0;
        var errors = new List<string>();
        foreach (var company in companies)
        {
            var (count, error) = await RefreshCompanyAsync(company, cancellationToken);
            added += count;
            if (error is not null) errors.Add(error);
        }

        return new RefreshResult(true, companies.Count, added, errors);
    }

    private async Task<(int Added, string? Error)> RefreshCompanyAsync(WatchedCompany company, CancellationToken cancellationToken)
    {
        var now = DbTime.UtcNow();
        try
        {
            var postings = await fetcher.FetchAsync(company.Board, company.Slug, cancellationToken);
            var known = (await db.JobRecs
                .Where(r => r.WatchedCompanyId == company.Id)
                .Select(r => r.ExternalId)
                .ToListAsync(cancellationToken)).ToHashSet();

            var menu = await db.Jobs.Select(j => new { j.Company, j.Role }).ToListAsync(cancellationToken);

            var added = 0;
            foreach (var posting in postings)
            {
                if (string.IsNullOrEmpty(posting.ExternalId) || known.Contains(posting.ExternalId)) continue;
                if (posting.PostedAt is { } posted && posted < DateTime.UtcNow - MaxAge) continue;

                var priority = RecMatcher.Score(posting.Title, posting.Location);
                if (priority == 0) continue;
                if (menu.Any(j => RecMatcher.IsSameJob(company.Name, posting.Title, j.Company, j.Role))) continue;

                db.JobRecs.Add(new JobRec(0, company.Id, posting.ExternalId, company.Name, Trim(posting.Title, 300),
                    Trim(posting.Location, 300), Trim(posting.Url, 500), posting.PostedAt is { } p ? DbTime.Round(p) : null,
                    now, priority, RecState.New, null));
                known.Add(posting.ExternalId);
                added++;
            }

            db.Entry(company).CurrentValues.SetValues(company with { LastCheckedAt = now, LastError = null });
            await db.SaveChangesAsync(cancellationToken);
            return (added, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JobBoardNotFoundException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Couldn't check {Company}", company.Name);
            var message = ex is JobBoardNotFoundException ? ex.Message : "Couldn't reach the job board.";
            db.Entry(company).CurrentValues.SetValues(company with { LastCheckedAt = now, LastError = message });
            await db.SaveChangesAsync(cancellationToken);
            return (0, $"{company.Name}: {message}");
        }
    }

    // Pin a rec to the menu as a "Beans picked" job
    public async Task<Job?> AddToMenuAsync(int recId)
    {
        var rec = await db.JobRecs.FindAsync(recId);
        if (rec is null || rec.State != RecState.New) return null;

        var job = await jobs.CreateAsync(new CreateJobRequest(
            rec.Company, rec.Title, rec.Location, JobStatus.Saved, null, rec.Url, "Found on the bulletin board"));

        db.Entry(rec).CurrentValues.SetValues(rec with { State = RecState.Added, JobId = job.Id });
        await db.SaveChangesAsync();
        return job;
    }

    // Take a rec off the board
    public async Task<bool> TossAsync(int recId)
    {
        var updated = await db.JobRecs
            .Where(r => r.Id == recId && r.State == RecState.New)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.State, RecState.Tossed));
        return updated > 0;
    }

    // Watch a new company; checks its board right away
    public async Task<(WatchedCompany? Company, string? Error)> AddCompanyAsync(AddCompanyRequest request, CancellationToken cancellationToken)
    {
        JobBoard board;
        string slug;

        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            if (!BoardLink.TryParse(request.Url, out board, out slug))
            {
                return (null, "That link isn't a Greenhouse, Lever, Ashby, or SmartRecruiters careers page.");
            }
        }
        else if (request.Board is { } chosen && BoardLink.IsValidSlug(request.Slug))
        {
            board = chosen;
            slug = request.Slug!.Trim();
        }
        else
        {
            return (null, "Paste a careers page link, or pick a board and company id.");
        }

        if (await db.WatchedCompanies.AnyAsync(c => c.Board == board && c.Slug.ToLower() == slug.ToLower(), cancellationToken))
        {
            return (null, "That company is already on the watchlist.");
        }

        try
        {
            await fetcher.FetchAsync(board, slug, cancellationToken);
        }
        catch (JobBoardNotFoundException ex)
        {
            return (null, ex.Message);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, "Couldn't reach that job board. Try again in a bit.");
        }

        var name = string.IsNullOrWhiteSpace(request.Name) ? PrettyName(slug) : request.Name.Trim();
        var company = new WatchedCompany(0, Trim(name, 100), board, slug, DbTime.UtcNow(), null, null);
        db.WatchedCompanies.Add(company);
        await db.SaveChangesAsync(cancellationToken);

        await RefreshCompanyAsync(company, cancellationToken);
        return (await db.WatchedCompanies.FindAsync([company.Id], cancellationToken), null);
    }

    public async Task<bool> RemoveCompanyAsync(int id)
    {
        return await db.WatchedCompanies.Where(c => c.Id == id).ExecuteDeleteAsync() > 0;
    }

    // "western-digital" -> "Western Digital"
    private static string PrettyName(string slug)
    {
        var words = slug.Replace('-', ' ').Replace('_', ' ').Replace('.', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + w[1..]);
        return string.Join(' ', words);
    }

    private static string Trim(string value, int max) => value.Length <= max ? value : value[..max];
}
