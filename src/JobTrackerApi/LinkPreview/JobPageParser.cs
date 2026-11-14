using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

// Reads job details out of a posting page's HTML
public static partial class JobPageParser
{
    private const int MaxDescriptionLength = 10000;

    public static JobLinkPreview Parse(string html, Uri url)
    {
        var document = new HtmlParser().ParseDocument(html);

        // Best source: the JobPosting data job sites add for Google Jobs
        var posting = FindJobPosting(document);

        var role = Clean(GetString(posting, "title"));
        var company = Clean(GetCompany(posting));
        var location = Clean(GetLocation(posting));
        var description = HtmlToText(GetString(posting, "description"));

        // Fallbacks: the page's own title and summary tags
        role ??= Clean(FirstPart(Meta(document, "og:title") ?? Meta(document, "twitter:title") ?? document.Title));
        company ??= Clean(Meta(document, "og:site_name"));
        description ??= Clean(Meta(document, "og:description") ?? Meta(document, "description"));

        return new JobLinkPreview(url.ToString(), company, role, location, Truncate(description));
    }

    // Finds a JSON-LD object with "@type": "JobPosting", even inside lists or @graph
    private static JsonElement? FindJobPosting(IDocument document)
    {
        foreach (var script in document.QuerySelectorAll("script[type='application/ld+json']"))
        {
            try
            {
                using var json = JsonDocument.Parse(script.TextContent);
                var found = Search(json.RootElement);
                if (found is not null)
                {
                    return found.Value.Clone();
                }
            }
            catch (JsonException)
            {
                // Broken JSON on the page, try the next one
            }
        }

        return null;
    }

    private static JsonElement? Search(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var found = Search(item);
                if (found is not null) return found;
            }
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("@type", out var type) && IsJobPosting(type))
            {
                return element;
            }

            if (element.TryGetProperty("@graph", out var graph))
            {
                return Search(graph);
            }
        }

        return null;
    }

    private static bool IsJobPosting(JsonElement type)
    {
        return type.ValueKind switch
        {
            JsonValueKind.String => type.GetString() == "JobPosting",
            JsonValueKind.Array => type.EnumerateArray().Any(t => t.ValueKind == JsonValueKind.String && t.GetString() == "JobPosting"),
            _ => false
        };
    }

    private static string? GetString(JsonElement? element, string name)
    {
        if (element is null || !element.Value.TryGetProperty(name, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static string? GetCompany(JsonElement? posting)
    {
        if (posting is null || !posting.Value.TryGetProperty("hiringOrganization", out var org)) return null;
        return org.ValueKind switch
        {
            JsonValueKind.String => org.GetString(),
            JsonValueKind.Object => GetString(org, "name"),
            _ => null
        };
    }

    private static string? GetLocation(JsonElement? posting)
    {
        if (posting is null) return null;

        if (GetString(posting, "jobLocationType") == "TELECOMMUTE") return "Remote";

        if (!posting.Value.TryGetProperty("jobLocation", out var location)) return null;

        // jobLocation can be one place or a list of places; use the first
        if (location.ValueKind == JsonValueKind.Array)
        {
            location = location.EnumerateArray().FirstOrDefault();
        }

        if (location.ValueKind != JsonValueKind.Object || !location.TryGetProperty("address", out var address))
        {
            return null;
        }

        if (address.ValueKind == JsonValueKind.String) return address.GetString();

        var parts = new[] { GetString(address, "addressLocality"), GetString(address, "addressRegion") }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var joined = string.Join(", ", parts);
        return joined.Length > 0 ? joined : GetString(address, "addressCountry");
    }

    private static string? Meta(IDocument document, string name)
    {
        var tag = document.QuerySelector($"meta[property='{name}']") ?? document.QuerySelector($"meta[name='{name}']");
        return tag?.GetAttribute("content");
    }

    // "Software Engineer | Some Site" -> "Software Engineer"
    private static string? FirstPart(string? title)
    {
        return title?.Split(" | ")[0];
    }

    // Turns description HTML into readable text with line breaks
    private static string? HtmlToText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;

        // Some sites escape the HTML twice (&lt;p&gt;)
        var decoded = html.Contains("&lt;") ? WebUtility.HtmlDecode(html) : html;
        var fragment = new HtmlParser().ParseDocument($"<body>{decoded}</body>").Body;
        if (fragment is null) return null;

        var text = new StringBuilder();
        AppendText(fragment, text);
        return Clean(ExtraBlankLines().Replace(text.ToString(), "\n\n"));
    }

    private static void AppendText(INode node, StringBuilder text)
    {
        foreach (var child in node.ChildNodes)
        {
            if (child is IText textNode)
            {
                text.Append(Whitespace().Replace(textNode.Data, " "));
                continue;
            }

            if (child is not IElement element) continue;

            var tag = element.LocalName;
            if (tag is "script" or "style") continue;
            if (tag == "br") { text.Append('\n'); continue; }
            if (tag == "li") text.Append("\n• ");

            AppendText(element, text);

            if (tag is "p" or "div" or "ul" or "ol" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6")
            {
                text.Append("\n\n");
            }
        }
    }

    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var lines = value.Split('\n').Select(line => line.Trim());
        var cleaned = string.Join("\n", lines).Trim();
        return cleaned.Length > 0 ? WebUtility.HtmlDecode(cleaned) : null;
    }

    private static string? Truncate(string? value)
    {
        if (value is null || value.Length <= MaxDescriptionLength) return value;
        return value[..MaxDescriptionLength].TrimEnd() + "…";
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExtraBlankLines();
}
