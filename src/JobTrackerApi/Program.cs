using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

// `dotnet run -- hash-password` makes a password hash instead of starting the API
if (args is ["hash-password"])
{
    HashPasswordCommand.Run();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// API docs
builder.Services.AddOpenApi();

// Connect to Postgres
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention());

// Owner login (only I can see and edit jobs)
builder.AddOwnerAuth();

// App logic, one per request
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<InterviewService>();
builder.Services.AddScoped<StatsService>();

// Job posting link reader (public sites only, 10 second limit)
builder.Services.AddHttpClient(LinkPreviewService.ClientName, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; JobTrackerLinkPreview/1.0)");
    })
    .ConfigurePrimaryHttpMessageHandler(SafeAddresses.CreateHandler);
builder.Services.AddScoped<LinkPreviewService>();

// Show enums as text in JSON
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// API docs in development only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Check tokens, then limit login attempts
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Routes
app.MapAuthEndpoints();
app.MapJobEndpoints();
app.MapInterviewEndpoints();
app.MapStatsEndpoints();

app.Run();

// Lets the test project see Program
public partial class Program { }
