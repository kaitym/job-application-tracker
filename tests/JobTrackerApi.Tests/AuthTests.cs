using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JobTrackerApi.Tests;

// Tests for login and who can see what
public class AuthTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/jobs")]
    [InlineData("/jobs/1")]
    [InlineData("/jobs/1/history")]
    [InlineData("/jobs/gone-cold")]
    [InlineData("/jobs/1/interviews")]
    [InlineData("/interviews/upcoming")]
    public async Task PrivateRoutes_WithoutToken_ReturnUnauthorized(string route)
    {
        var response = await _factory.CreateClient().GetAsync(route);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostJob_WithoutToken_ReturnsUnauthorized()
    {
        var request = new CreateJobRequest("Sneaky Co", "Hacker", "Nowhere", JobStatus.Saved, null, null, null);

        var response = await _factory.CreateClient().PostAsJsonAsync("/jobs", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_WithoutToken_IsPublic()
    {
        var response = await _factory.CreateClient().GetAsync("/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/auth/login",
            new LoginRequest(ApiFactory.OwnerUsername, "not-the-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongUsername_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/auth/login",
            new LoginRequest("someone-else", ApiFactory.OwnerPassword));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_RightPassword_ReturnsWorkingToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login",
            new LoginRequest(ApiFactory.OwnerUsername, ApiFactory.OwnerPassword));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.True(login.ExpiresAt > DateTime.UtcNow);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var jobs = await client.GetAsync("/jobs");
        Assert.Equal(HttpStatusCode.OK, jobs.StatusCode);
    }

    [Fact]
    public async Task ForgedToken_WithWrongKey_IsRejected()
    {
        var forged = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = TokenService.Issuer,
            Audience = TokenService.Issuer,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, ApiFactory.OwnerUsername)]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a-completely-different-key-that-is-long-enough!!")),
                SecurityAlgorithms.HmacSha256)
        });

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", forged);

        var response = await client.GetAsync("/jobs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
