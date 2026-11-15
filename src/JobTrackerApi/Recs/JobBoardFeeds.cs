using System.Net;
using System.Text.Json;

// Thrown when a company isn't on that board
public class JobBoardNotFoundException(JobBoard board, string slug)
    : Exception($"No {board} job board found for \"{slug}\".");

// Reads a company's postings from its job board
public interface IJobBoardFetcher
{
    Task<IReadOnlyList<RemotePosting>> FetchAsync(JobBoard board, string slug, CancellationToken cancellationToken);
}

public class JobBoardFetcher(IHttpClientFactory httpClientFactory) : IJobBoardFetcher
{
    public const string ClientName = "job-boards";

    public async Task<IReadOnlyList<RemotePosting>> FetchAsync(JobBoard board, string slug, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(ClientName);
        var id = Uri.EscapeDataString(slug);

        switch (board)
        {
            case JobBoard.Greenhouse:
                return JobBoardFeeds.ParseGreenhouse(await GetJsonAsync(client, $"https://boards-api.greenhouse.io/v1/boards/{id}/jobs", board, slug, cancellationToken));
            case JobBoard.Lever:
                return JobBoardFeeds.ParseLever(await GetJsonAsync(client, $"https://api.lever.co/v0/postings/{id}?mode=json", board, slug, cancellationToken));
            case JobBoard.Ashby:
                return JobBoardFeeds.ParseAshby(await GetJsonAsync(client, $"https://api.ashbyhq.com/posting-api/job-board/{id}", board, slug, cancellationToken));
            case JobBoard.SmartRecruiters:
                // Pages of 100, up to 500 postings
                var postings = new List<RemotePosting>();
                for (var offset = 0; offset < 500; offset += 100)
                {
                    using var page = await GetJsonAsync(client, $"https://api.smartrecruiters.com/v1/companies/{id}/postings?limit=100&offset={offset}", board, slug, cancellationToken);
                    var batch = JobBoardFeeds.ParseSmartRecruiters(page, slug);
                    postings.AddRange(batch);
                    if (batch.Count < 100) break;
                }

                if (postings.Count == 0) await EnsureSmartRecruitersCompanyAsync(client, id, board, slug, cancellationToken);
                return postings;
            default:
                throw new ArgumentOutOfRangeException(nameof(board));
        }
    }

    private static async Task<JsonDocument> GetJsonAsync(HttpClient client, string url, JobBoard board, string slug, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) throw new JobBoardNotFoundException(board, slug);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    // SmartRecruiters answers 200 with no postings for unknown companies; the company endpoint 404s
    private static async Task EnsureSmartRecruitersCompanyAsync(HttpClient client, string id, JobBoard board, string slug, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"https://api.smartrecruiters.com/v1/companies/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode) throw new JobBoardNotFoundException(board, slug);
    }
}

// Turns each board's JSON into postings (no network, so it's easy to test)
public static class JobBoardFeeds
{
    public static List<RemotePosting> ParseGreenhouse(JsonDocument json)
    {
        var postings = new List<RemotePosting>();
        if (!json.RootElement.TryGetProperty("jobs", out var jobs)) return postings;

        foreach (var job in jobs.EnumerateArray())
        {
            var location = job.TryGetProperty("location", out var loc) ? Str(loc, "name") : null;
            postings.Add(new RemotePosting(
                Num(job, "id"),
                Str(job, "title") ?? "",
                location ?? "",
                Str(job, "absolute_url") ?? "",
                Date(Str(job, "first_published") ?? Str(job, "updated_at"))));
        }

        return postings;
    }

    public static List<RemotePosting> ParseLever(JsonDocument json)
    {
        var postings = new List<RemotePosting>();
        if (json.RootElement.ValueKind != JsonValueKind.Array) return postings;

        foreach (var job in json.RootElement.EnumerateArray())
        {
            var location = "";
            if (job.TryGetProperty("categories", out var categories))
            {
                location = categories.TryGetProperty("allLocations", out var all) && all.ValueKind == JsonValueKind.Array
                    ? string.Join("; ", all.EnumerateArray().Select(l => l.GetString()))
                    : Str(categories, "location") ?? "";
            }

            DateTime? posted = job.TryGetProperty("createdAt", out var created) && created.TryGetInt64(out var ms)
                ? DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime
                : null;

            postings.Add(new RemotePosting(Str(job, "id") ?? "", Str(job, "text") ?? "", location, Str(job, "hostedUrl") ?? "", posted));
        }

        return postings;
    }

    public static List<RemotePosting> ParseAshby(JsonDocument json)
    {
        var postings = new List<RemotePosting>();
        if (!json.RootElement.TryGetProperty("jobs", out var jobs)) return postings;

        foreach (var job in jobs.EnumerateArray())
        {
            var location = Str(job, "location") ?? "";
            if (job.TryGetProperty("isRemote", out var remote) && remote.ValueKind == JsonValueKind.True && !location.Contains("remote", StringComparison.OrdinalIgnoreCase))
            {
                location += " (Remote)";
            }

            postings.Add(new RemotePosting(Str(job, "id") ?? "", Str(job, "title") ?? "", location, Str(job, "jobUrl") ?? "", Date(Str(job, "publishedAt"))));
        }

        return postings;
    }

    public static List<RemotePosting> ParseSmartRecruiters(JsonDocument json, string slug)
    {
        var postings = new List<RemotePosting>();
        if (!json.RootElement.TryGetProperty("content", out var content)) return postings;

        foreach (var job in content.EnumerateArray())
        {
            var location = "";
            if (job.TryGetProperty("location", out var loc))
            {
                location = Str(loc, "fullLocation") ?? string.Join(", ", new[] { Str(loc, "city"), Str(loc, "region") }.Where(p => !string.IsNullOrEmpty(p)));
                if (loc.TryGetProperty("remote", out var remote) && remote.ValueKind == JsonValueKind.True) location += " (Remote)";
            }

            var id = Str(job, "id") ?? "";
            postings.Add(new RemotePosting(id, Str(job, "name") ?? "", location, $"https://jobs.smartrecruiters.com/{slug}/{id}", Date(Str(job, "releasedDate"))));
        }

        return postings;
    }

    private static string? Str(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string Num(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return "";
        return value.ValueKind == JsonValueKind.Number ? value.GetRawText() : value.GetString() ?? "";
    }

    private static DateTime? Date(string? value)
    {
        return DateTimeOffset.TryParse(value, out var date) ? date.UtcDateTime : null;
    }
}
