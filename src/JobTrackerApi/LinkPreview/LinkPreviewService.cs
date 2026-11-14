using System.Net;
using System.Net.Http.Headers;

// Fetches a job posting page and reads what it can from it
public class LinkPreviewService(IHttpClientFactory httpClientFactory)
{
    public const string ClientName = "link-preview";
    private const int MaxPageBytes = 3 * 1024 * 1024;

    public async Task<LinkPreviewResult> PreviewAsync(string? rawUrl, CancellationToken cancellationToken)
    {
        // Only real web links
        if (!Uri.TryCreate(rawUrl?.Trim(), UriKind.Absolute, out var url)
            || (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp))
        {
            return LinkPreviewResult.Invalid("That doesn't look like a web link.");
        }

        if (!url.IsDefaultPort || url.IsLoopback || (IPAddress.TryParse(url.Host, out var ip) && SafeAddresses.IsBlocked(ip)))
        {
            return LinkPreviewResult.Invalid("That link points somewhere private.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            var client = httpClientFactory.CreateClient(ClientName);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return LinkPreviewResult.Unreadable($"The site answered {(int)response.StatusCode}.");
            }

            if (response.Content.Headers.ContentType?.MediaType is { } type && !type.Contains("html"))
            {
                return LinkPreviewResult.Unreadable("That link isn't a web page.");
            }

            var html = await ReadLimitedAsync(response.Content, cancellationToken);
            return LinkPreviewResult.Ok(JobPageParser.Parse(html, url));
        }
        catch (HttpRequestException ex) when (ex.InnerException is BlockedAddressException)
        {
            return LinkPreviewResult.Invalid("That link points somewhere private.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return LinkPreviewResult.Unreadable("Couldn't reach that page.");
        }
    }

    // Reads at most MaxPageBytes so a huge page can't eat memory
    private static async Task<string> ReadLimitedAsync(HttpContent content, CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[MaxPageBytes];
        var total = 0;
        int read;
        while (total < buffer.Length && (read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken)) > 0)
        {
            total += read;
        }

        var charset = content.Headers.ContentType?.CharSet;
        var encoding = System.Text.Encoding.UTF8;
        try
        {
            if (!string.IsNullOrWhiteSpace(charset)) encoding = System.Text.Encoding.GetEncoding(charset.Trim('"'));
        }
        catch (ArgumentException)
        {
            // Unknown charset, stick with UTF-8
        }

        return encoding.GetString(buffer, 0, total);
    }
}

// Either a preview, or why there isn't one
public record LinkPreviewResult(JobLinkPreview? Preview, string? Error, bool IsInvalidLink)
{
    public static LinkPreviewResult Ok(JobLinkPreview preview) => new(preview, null, false);
    public static LinkPreviewResult Invalid(string error) => new(null, error, true);
    public static LinkPreviewResult Unreadable(string error) => new(null, error, false);
}
