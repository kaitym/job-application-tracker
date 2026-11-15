using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobTrackerApi.Tests;

// A pretend job board, so tests never hit the internet
public class FakeJobBoardFetcher : IJobBoardFetcher
{
    public Dictionary<string, List<RemotePosting>> Boards { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<RemotePosting>> FetchAsync(JobBoard board, string slug, CancellationToken cancellationToken)
    {
        if (!Boards.TryGetValue(slug, out var postings)) throw new JobBoardNotFoundException(board, slug);
        return Task.FromResult<IReadOnlyList<RemotePosting>>(postings.ToList());
    }
}

// The API with the pretend job board plugged in
public class RecsApiFactory : ApiFactory
{
    public FakeJobBoardFetcher Fetcher { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IJobBoardFetcher>();
            services.AddSingleton<IJobBoardFetcher>(Fetcher);
        });
    }
}

// Tests for the bulletin board
public class RecsEndpointsTests : IClassFixture<RecsApiFactory>
{
    private readonly RecsApiFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = TestJson.Options;

    public RecsEndpointsTests(RecsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateOwnerClient();
    }

    private static RemotePosting Posting(string id, string title, string location, int daysAgo = 2) =>
        new(id, title, location, $"https://example.com/jobs/{id}", DateTime.UtcNow.AddDays(-daysAgo));

    [Fact]
    public async Task AddCompany_PinsOnlyMatchingRoles_BestFirst()
    {
        _factory.Fetcher.Boards["wavelabs"] =
        [
            Posting("1", "Software Engineer, New Grad", "San Francisco, CA"),
            Posting("2", "Software Engineer I", "Irvine, CA"),
            Posting("3", "Senior Software Engineer", "Irvine, CA"),
            Posting("4", "Marketing Associate", "Irvine, CA"),
            Posting("5", "Software Engineer, New Grad", "London, UK"),
            Posting("6", "Software Engineer, Early Career", "San Diego, CA", daysAgo: 120), // too old
        ];

        var response = await _client.PostAsJsonAsync("/recs/companies",
            new AddCompanyRequest("https://boards.greenhouse.io/wavelabs", null, null, "Wave Labs"), JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var board = await GetBoardAsync();
        var recs = board.Recs.Where(r => r.Company == "Wave Labs").ToList();

        Assert.Equal(["2", "1"], recs.Select(r => r.ExternalId));
        Assert.Equal(3, recs[0].Priority);
    }

    [Fact]
    public async Task AddCompany_UnknownBoard_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/recs/companies",
            new AddCompanyRequest("https://jobs.lever.co/not-a-real-company", null, null, null), JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddCompany_NotABoardLink_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/recs/companies",
            new AddCompanyRequest("https://www.linkedin.com/jobs/view/1", null, null, null), JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_AddsNewPostingsWithoutDuplicates()
    {
        _factory.Fetcher.Boards["tidesystems"] = [Posting("a", "Junior Developer", "Costa Mesa, CA")];
        await _client.PostAsJsonAsync("/recs/companies", new AddCompanyRequest(null, JobBoard.Lever, "tidesystems", "Tide Systems"), JsonOptions);

        _factory.Fetcher.Boards["tidesystems"].Add(Posting("b", "Associate Software Engineer", "San Diego, CA"));

        var refresh = await _client.PostAsync("/recs/refresh", null);
        var result = await refresh.Content.ReadFromJsonAsync<RefreshResult>(JsonOptions);
        Assert.True(result!.Ran);

        // Nothing new the second time
        await _client.PostAsync("/recs/refresh", null);

        var board = await GetBoardAsync();
        Assert.Equal(["a", "b"], board.Recs.Where(r => r.Company == "Tide Systems").Select(r => r.ExternalId).Order());
    }

    [Fact]
    public async Task RefreshIfStale_SkipsWhenRecentlyChecked()
    {
        _factory.Fetcher.Boards["freshco"] = [];
        await _client.PostAsJsonAsync("/recs/companies", new AddCompanyRequest(null, JobBoard.Ashby, "freshco", null), JsonOptions);

        var response = await _client.PostAsync("/recs/refresh?onlyIfStale=true", null);
        var result = await response.Content.ReadFromJsonAsync<RefreshResult>(JsonOptions);

        Assert.False(result!.Ran);
    }

    [Fact]
    public async Task AddRecToMenu_CreatesSavedJob_AndUnpinsIt()
    {
        _factory.Fetcher.Boards["pierlabs"] = [Posting("p1", "Software Engineer, New Grad", "Irvine, CA")];
        await _client.PostAsJsonAsync("/recs/companies", new AddCompanyRequest(null, JobBoard.Greenhouse, "pierlabs", "Pier Labs"), JsonOptions);
        var rec = (await GetBoardAsync()).Recs.Single(r => r.Company == "Pier Labs");

        var response = await _client.PostAsync($"/recs/{rec.Id}/add", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var job = await response.Content.ReadFromJsonAsync<Job>(JsonOptions);

        Assert.Equal("Pier Labs", job!.Company);
        Assert.Equal(JobStatus.Saved, job.Status);
        Assert.Equal("https://example.com/jobs/p1", job.Link);
        Assert.DoesNotContain((await GetBoardAsync()).Recs, r => r.Id == rec.Id);

        // Can't add it twice
        var again = await _client.PostAsync($"/recs/{rec.Id}/add", null);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
    }

    [Fact]
    public async Task TossRec_TakesItOffTheBoard()
    {
        _factory.Fetcher.Boards["tossco"] = [Posting("t1", "Software Engineer I", "San Diego, CA")];
        await _client.PostAsJsonAsync("/recs/companies", new AddCompanyRequest(null, JobBoard.Greenhouse, "tossco", null), JsonOptions);
        var rec = (await GetBoardAsync()).Recs.Single(r => r.Company == "Tossco");

        var response = await _client.PostAsync($"/recs/{rec.Id}/toss", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.DoesNotContain((await GetBoardAsync()).Recs, r => r.Id == rec.Id);
    }

    [Fact]
    public async Task RemoveCompany_RemovesItsRecs()
    {
        _factory.Fetcher.Boards["goneco"] = [Posting("g1", "Software Engineer I", "Irvine, CA")];
        var created = await _client.PostAsJsonAsync("/recs/companies", new AddCompanyRequest(null, JobBoard.Greenhouse, "goneco", null), JsonOptions);
        var company = await created.Content.ReadFromJsonAsync<WatchedCompany>(JsonOptions);

        var response = await _client.DeleteAsync($"/recs/companies/{company!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var board = await GetBoardAsync();
        Assert.DoesNotContain(board.Companies, c => c.Id == company.Id);
        Assert.DoesNotContain(board.Recs, r => r.WatchedCompanyId == company.Id);
    }

    [Fact]
    public async Task Board_SkipsJobsAlreadyOnTheMenu()
    {
        await _client.PostAsJsonAsync("/jobs", new CreateJobRequest("Menu Co", "Software Engineer I, Growth", "Irvine, CA",
            JobStatus.Applied, null, null, null), JsonOptions);
        _factory.Fetcher.Boards["menuco"] =
        [
            Posting("m1", "Software Engineer I - Growth", "Irvine, CA"),
            Posting("m2", "Software Engineer I - Payments", "Irvine, CA"),
        ];

        await _client.PostAsJsonAsync("/recs/companies", new AddCompanyRequest(null, JobBoard.Greenhouse, "menuco", "Menu Co"), JsonOptions);

        var board = await GetBoardAsync();
        Assert.Equal(["m2"], board.Recs.Where(r => r.Company == "Menu Co").Select(r => r.ExternalId));
    }

    [Fact]
    public async Task Recs_WithoutLogin_ReturnUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/recs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<RecBoardResponse> GetBoardAsync()
    {
        return (await _client.GetFromJsonAsync<RecBoardResponse>("/recs", JsonOptions))!;
    }
}
