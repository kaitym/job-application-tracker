// /auth routes
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Log in as the owner, get a token
        app.MapPost("/auth/login", (LoginRequest request, TokenService tokens) =>
        {
            var result = tokens.Login(request);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        })
        .RequireRateLimiting(AuthSetup.LoginRateLimit);
    }
}
