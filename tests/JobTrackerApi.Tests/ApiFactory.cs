using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace JobTrackerApi.Tests;

// Starts the API against a throwaway Postgres in Docker
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string OwnerUsername = "test-owner";
    public const string OwnerPassword = "test-password-123";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17")
        .Build();

    private string _token = "";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());

        // Test login, so tests never depend on real settings
        builder.UseSetting("Auth:Username", OwnerUsername);
        builder.UseSetting("Auth:PasswordHash", Passwords.Hash(OwnerPassword));
        builder.UseSetting("Auth:JwtKey", "test-only-signing-key-that-is-long-enough-1234567890");
    }

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        // Build the tables
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        // Log in once and share the token
        var response = await CreateClient().PostAsJsonAsync("/auth/login",
            new LoginRequest(OwnerUsername, OwnerPassword));
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _token = login!.Token;
    }

    // A client that's logged in as the owner
    public HttpClient CreateOwnerClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _db.DisposeAsync();
        await base.DisposeAsync();
    }
}
