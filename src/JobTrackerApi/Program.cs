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

// List all jobs
app.MapGet("/jobs", () => jobs);

// Add a job
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

// Get one job
app.MapGet("/jobs/{id}", (int id) =>
{
    var job = jobs.FirstOrDefault(j => j.Id == id);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

// Update a job
app.MapPut("/jobs/{id}", (int id, UpdateJobRequest request) =>
{
    var index = jobs.FindIndex(j => j.Id == id);
    if (index == -1)
    {
        return Results.NotFound();
    }

    if (string.IsNullOrWhiteSpace(request.Company) || string.IsNullOrWhiteSpace(request.Role))
    {
        return Results.BadRequest("Company and Role are required.");
    }

    var updated = jobs[index] with
    {
        Company = request.Company,
        Role = request.Role,
        Location = request.Location,
        Status = request.Status,
        DateApplied = request.DateApplied,
        Link = request.Link,
        Notes = request.Notes
    };

    jobs[index] = updated;
    return Results.Ok(updated);
});

// Delete a job
app.MapDelete("/jobs/{id}", (int id) =>
{
    var removed = jobs.RemoveAll(j => j.Id == id);
    return removed == 0 ? Results.NotFound() : Results.NoContent();
});

app.Run();

// Lets the test project see Program
public partial class Program { }
