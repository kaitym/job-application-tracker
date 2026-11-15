using System.Text.RegularExpressions;

// Turns a careers page link into a board and company id
public static partial class BoardLink
{
    public static bool TryParse(string? url, out JobBoard board, out string slug)
    {
        board = default;
        slug = "";

        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)) return false;

        var host = uri.Host.ToLowerInvariant();
        var first = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

        // boards.greenhouse.io/pinterest?gh_jid=... uses ?for= on embeds
        if (host.EndsWith("greenhouse.io"))
        {
            board = JobBoard.Greenhouse;
            slug = first == "embed" ? GetQuery(uri, "for") ?? "" : first;
        }
        else if (host == "jobs.lever.co" || host == "api.lever.co")
        {
            board = JobBoard.Lever;
            slug = first == "v0" ? uri.Segments.ElementAtOrDefault(3)?.Trim('/') ?? "" : first;
        }
        else if (host == "jobs.ashbyhq.com")
        {
            board = JobBoard.Ashby;
            slug = first;
        }
        else if (host is "jobs.smartrecruiters.com" or "careers.smartrecruiters.com")
        {
            board = JobBoard.SmartRecruiters;
            slug = first;
        }
        else
        {
            return false;
        }

        return IsValidSlug(slug);
    }

    public static bool IsValidSlug(string? slug)
    {
        return !string.IsNullOrWhiteSpace(slug) && SlugPattern().IsMatch(slug);
    }

    private static string? GetQuery(Uri uri, string key)
    {
        foreach (var part in uri.Query.TrimStart('?').Split('&'))
        {
            var pieces = part.Split('=', 2);
            if (pieces.Length == 2 && pieces[0] == key) return Uri.UnescapeDataString(pieces[1]);
        }

        return null;
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,80}$")]
    private static partial Regex SlugPattern();
}
