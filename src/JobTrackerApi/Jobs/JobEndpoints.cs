// /jobs routes
public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        var jobs = app.MapGroup("/jobs");

        // List all jobs
        jobs.MapGet("", async (JobService service) =>
            Results.Ok(await service.GetAllAsync()));

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
        jobs.MapGet("/{id}", async (int id, JobService service) =>
        {
            var job = await service.GetByIdAsync(id);
            return job is null ? Results.NotFound() : Results.Ok(job);
        });

        // Update a job
        jobs.MapPut("/{id}", async (int id, UpdateJobRequest request, JobService service) =>
        {
            if (!HasRequiredFields(request.Company, request.Role))
            {
                return Results.BadRequest("Company and Role are required.");
            }

            var job = await service.UpdateAsync(id, request);
            return job is null ? Results.NotFound() : Results.Ok(job);
        });

        // Delete a job
        jobs.MapDelete("/{id}", async (int id, JobService service) =>
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
