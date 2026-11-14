using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// API docs
builder.Services.AddOpenApi();

// Connect to Postgres
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention());

// App logic, one per request
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<InterviewService>();
builder.Services.AddScoped<StatsService>();

// Show enums as text in JSON
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// API docs in development only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Routes
app.MapJobEndpoints();
app.MapInterviewEndpoints();
app.MapStatsEndpoints();

app.Run();

// Lets the test project see Program
public partial class Program { }
