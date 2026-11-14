// /stats routes
public static class StatsEndpoints
{
    public static void MapStatsEndpoints(this WebApplication app)
    {
        // Job search numbers and shop level (public: counts only, no company names)
        app.MapGet("/stats", async (StatsService service) =>
            Results.Ok(await service.GetAsync()))
            .AllowAnonymous();

        // Collect earned XP (owner only)
        app.MapPost("/stats/collect", async (StatsService service) =>
            Results.Ok(await service.CollectAsync()))
            .RequireAuthorization();
    }
}
