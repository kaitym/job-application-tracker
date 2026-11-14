using System.Net;
using System.Net.Http.Json;

namespace JobTrackerApi.Tests;

// Its own API instance, so these login attempts don't affect other tests
public class LoginRateLimitTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public LoginRateLimitTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_TooManyAttempts_ReturnsTooManyRequests()
    {
        var client = _factory.CreateClient();
        var statuses = new List<HttpStatusCode>();

        for (var i = 0; i < 6; i++)
        {
            var response = await client.PostAsJsonAsync("/auth/login",
                new LoginRequest(ApiFactory.OwnerUsername, "guess-" + i));
            statuses.Add(response.StatusCode);
        }

        Assert.Contains(HttpStatusCode.TooManyRequests, statuses);
    }
}
