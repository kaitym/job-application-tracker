// /stats route
public static class StatsEndpoints
{
    public static void MapStatsEndpoints(this WebApplication app)
    {
        // Job search numbers and shop level (public: counts only, no company names)
        app.MapGet("/stats", async (StatsService service) =>
            Results.Ok(await service.GetAsync()))
            .AllowAnonymous();
    }
}
