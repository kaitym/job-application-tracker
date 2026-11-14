using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);
var jobs = new List<Job>();
var nextId = 1;


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Show enums as text in JSON
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Endpoints; list and add jobs
app.MapGet ("/jobs", () => jobs);
app.MapPost("/jobs", (CreateJobRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Company) || string.IsNullOrWhiteSpace(request.Role))
    {
        return Results.BadRequest("Company and Role are required.");
    }

    var job = new Job(
        nextId++,
        request.Company,
        request.Role,
        request.Location,
        request.Status,
        request.DateApplied,
        request.Link,
        request.Notes
    );

    jobs.Add(job);
    return Results.Created($"/jobs/{job.Id}", job);
});

app.Run();

// Lets the test project see Program
public partial class Program { }
