using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Connect to Postgres
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention());

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
app.MapGet("/jobs", async (AppDbContext db) =>
    await db.Jobs.OrderBy(j => j.Id).ToListAsync());

// Add a job
app.MapPost("/jobs", async (CreateJobRequest request, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Company) || string.IsNullOrWhiteSpace(request.Role))
    {
        return Results.BadRequest("Company and Role are required.");
    }

    var job = new Job(
        0,
        request.Company,
        request.Role,
        request.Location,
        request.Status,
        request.DateApplied,
        request.Link,
        request.Notes
    );

    db.Jobs.Add(job);
    await db.SaveChangesAsync();
    return Results.Created($"/jobs/{job.Id}", job);
});

// Get one job
app.MapGet("/jobs/{id}", async (int id, AppDbContext db) =>
{
    var job = await db.Jobs.FindAsync(id);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

// Update a job
app.MapPut("/jobs/{id}", async (int id, UpdateJobRequest request, AppDbContext db) =>
{
    var existing = await db.Jobs.FindAsync(id);
    if (existing is null)
    {
        return Results.NotFound();
    }

    if (string.IsNullOrWhiteSpace(request.Company) || string.IsNullOrWhiteSpace(request.Role))
    {
        return Results.BadRequest("Company and Role are required.");
    }

    var updated = existing with
    {
        Company = request.Company,
        Role = request.Role,
        Location = request.Location,
        Status = request.Status,
        DateApplied = request.DateApplied,
        Link = request.Link,
        Notes = request.Notes
    };

    db.Entry(existing).CurrentValues.SetValues(updated);
    await db.SaveChangesAsync();
    return Results.Ok(updated);
});

// Delete a job
app.MapDelete("/jobs/{id}", async (int id, AppDbContext db) =>
{
    var removed = await db.Jobs.Where(j => j.Id == id).ExecuteDeleteAsync();
    return removed == 0 ? Results.NotFound() : Results.NoContent();
});

app.Run();

// Lets the test project see Program
public partial class Program { }
