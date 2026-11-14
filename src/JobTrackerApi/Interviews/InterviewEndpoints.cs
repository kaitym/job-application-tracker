// Interview routes
public static class InterviewEndpoints
{
    public static void MapInterviewEndpoints(this WebApplication app)
    {
        var jobInterviews = app.MapGroup("/jobs/{jobId:int}/interviews");

        // A job's interviews
        jobInterviews.MapGet("", async (int jobId, InterviewService service) =>
        {
            var interviews = await service.GetForJobAsync(jobId);
            return interviews is null ? Results.NotFound() : Results.Ok(interviews);
        });

        // Schedule an interview
        jobInterviews.MapPost("", async (int jobId, CreateInterviewRequest request, InterviewService service) =>
        {
            if (request.ScheduledAt == default)
            {
                return Results.BadRequest("ScheduledAt is required.");
            }

            var interview = await service.CreateAsync(jobId, request);
            return interview is null
                ? Results.NotFound()
                : Results.Created($"/jobs/{jobId}/interviews/{interview.Id}", interview);
        });

        // Cancel an interview
        jobInterviews.MapDelete("/{interviewId:int}", async (int jobId, int interviewId, InterviewService service) =>
        {
            var deleted = await service.DeleteAsync(jobId, interviewId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        // Interviews coming up soon, across all jobs
        app.MapGet("/interviews/upcoming", async (int? days, InterviewService service) =>
        {
            var window = days ?? 14;
            if (window < 1)
            {
                return Results.BadRequest("Days must be at least 1.");
            }

            return Results.Ok(await service.GetUpcomingAsync(window));
        });
    }
}
