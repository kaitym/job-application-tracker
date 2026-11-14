using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Wires up owner login: settings, token checks, and login rate limit
public static class AuthSetup
{
    public const string LoginRateLimit = "login";

    public static void AddOwnerAuth(this WebApplicationBuilder builder)
    {
        var settings = AuthSettings.Load(builder.Configuration);
        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton<TokenService>();

        // Check "Authorization: Bearer <token>" on every request
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = TokenService.Issuer,
                    ValidAudience = TokenService.Issuer,
                    IssuerSigningKey = TokenService.SigningKey(settings),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        builder.Services.AddAuthorization();

        // Slow down password guessing: 5 login tries per minute per IP
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(LoginRateLimit, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });
    }
}
