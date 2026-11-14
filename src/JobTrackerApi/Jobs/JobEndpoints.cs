// /jobs routes
public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        var jobs = app.MapGroup("/jobs");

        // List jobs, with optional filters
        jobs.MapGet("", async (JobStatus? status, string? search, bool? needsAction, JobService service) =>
            Results.Ok(await service.GetAllAsync(status, search, needsAction)));

        // Applied jobs with no news in a while
        jobs.MapGet("/gone-cold", async (int? days, JobService service) =>
        {
            var cutoffDays = days ?? 30;
            if (cutoffDays < 1)
            {
                return Results.BadRequest("Days must be at least 1.");
            }

            return Results.Ok(await service.GetGoneColdAsync(cutoffDays));
        });

        // Add a job
        jobs.MapPost("", async (CreateJobRequest request, JobService service) =>
        {
            if (!HasRequiredFields(request.Company, request.Role))
            {
                return Results.BadRequest("Company and Role are required.");
            }

            var job = await service.CreateAsync(request);
            return Results.Created($"/jobs/{job.Id}", job);
        });

        // Get one job
        jobs.MapGet("/{id:int}", async (int id, JobService service) =>
        {
            var job = await service.GetByIdAsync(id);
            return job is null ? Results.NotFound() : Results.Ok(job);
        });

        // A job's status history
        jobs.MapGet("/{id:int}/history", async (int id, JobService service) =>
        {
            var history = await service.GetHistoryAsync(id);
            return history is null ? Results.NotFound() : Results.Ok(history);
        });

        // Update a job
        jobs.MapPut("/{id:int}", async (int id, UpdateJobRequest request, JobService service) =>
        {
            if (!HasRequiredFields(request.Company, request.Role))
            {
                return Results.BadRequest("Company and Role are required.");
            }

            var job = await service.UpdateAsync(id, request);
            return job is null ? Results.NotFound() : Results.Ok(job);
        });

        // Delete a job
        jobs.MapDelete("/{id:int}", async (int id, JobService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    // Company and role can't be blank
    private static bool HasRequiredFields(string company, string role)
    {
        return !string.IsNullOrWhiteSpace(company) && !string.IsNullOrWhiteSpace(role);
    }
}
