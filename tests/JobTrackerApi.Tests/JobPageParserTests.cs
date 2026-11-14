namespace JobTrackerApi.Tests;

// Unit tests for reading job details out of posting pages (no network)
public class JobPageParserTests
{
    private static readonly Uri Url = new("https://jobs.example.com/123");

    [Fact]
    public void Parse_JobPostingData_FillsEverything()
    {
        var html = """
            <html><head><title>Ignore me</title>
            <script type="application/ld+json">
            {
              "@context": "https://schema.org",
              "@type": "JobPosting",
              "title": "Software Engineer, New Grad",
              "hiringOrganization": { "@type": "Organization", "name": "Wave Labs" },
              "jobLocation": [{ "@type": "Place", "address": { "addressLocality": "Irvine", "addressRegion": "CA" } }],
              "description": "<p>Build things.</p><ul><li>C#</li><li>Postgres</li></ul>"
            }
            </script></head><body></body></html>
            """;

        var preview = JobPageParser.Parse(html, Url);

        Assert.Equal("Software Engineer, New Grad", preview.Role);
        Assert.Equal("Wave Labs", preview.Company);
        Assert.Equal("Irvine, CA", preview.Location);
        Assert.Equal("Build things.\n\n• C#\n• Postgres", preview.Description);
        Assert.Equal(Url.ToString(), preview.Link);
    }

    [Fact]
    public void Parse_JobPostingInsideGraph_IsFound()
    {
        var html = """
            <script type="application/ld+json">
            { "@graph": [ { "@type": "WebPage" }, { "@type": ["JobPosting"], "title": "Backend Engineer",
              "hiringOrganization": "Pier Co", "jobLocationType": "TELECOMMUTE" } ] }
            </script>
            """;

        var preview = JobPageParser.Parse(html, Url);

        Assert.Equal("Backend Engineer", preview.Role);
        Assert.Equal("Pier Co", preview.Company);
        Assert.Equal("Remote", preview.Location);
    }

    [Fact]
    public void Parse_EscapedDescriptionHtml_BecomesText()
    {
        var html = """
            <script type="application/ld+json">
            { "@type": "JobPosting", "title": "SWE", "description": "&lt;p&gt;Surf &amp;amp; code&lt;/p&gt;" }
            </script>
            """;

        var preview = JobPageParser.Parse(html, Url);

        Assert.Equal("Surf & code", preview.Description);
    }

    [Fact]
    public void Parse_NoJobData_FallsBackToPageTags()
    {
        var html = """
            <html><head>
            <meta property="og:title" content="Junior Developer | Tide Systems Careers">
            <meta property="og:site_name" content="Tide Systems">
            <meta name="description" content="Join our team by the beach.">
            </head></html>
            """;

        var preview = JobPageParser.Parse(html, Url);

        Assert.Equal("Junior Developer", preview.Role);
        Assert.Equal("Tide Systems", preview.Company);
        Assert.Null(preview.Location);
        Assert.Equal("Join our team by the beach.", preview.Description);
    }

    [Fact]
    public void Parse_BrokenJsonAndEmptyPage_ReturnsNothingButTheLink()
    {
        var html = """<script type="application/ld+json">{ not json </script>""";

        var preview = JobPageParser.Parse(html, Url);

        Assert.Null(preview.Role);
        Assert.Null(preview.Company);
        Assert.Null(preview.Description);
        Assert.Equal(Url.ToString(), preview.Link);
    }
}
