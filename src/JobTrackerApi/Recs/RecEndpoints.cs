// Bulletin board routes (owner only)
public static class RecEndpoints
{
    public static void MapRecEndpoints(this WebApplication app)
    {
        var recs = app.MapGroup("/recs").RequireAuthorization();

        // What's pinned, plus the watchlist
        recs.MapGet("", async (RecService service) => Results.Ok(await service.GetBoardAsync()));

        // Check the job boards (?onlyIfStale=true skips it if checked in the last 12 hours)
        recs.MapPost("/refresh", async (bool? onlyIfStale, RecService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.RefreshAsync(onlyIfStale ?? false, cancellationToken)));

        // Add a rec to the menu
        recs.MapPost("/{id:int}/add", async (int id, RecService service) =>
        {
            var job = await service.AddToMenuAsync(id);
            return job is null ? Results.NotFound() : Results.Ok(job);
        });

        // Toss a rec
        recs.MapPost("/{id:int}/toss", async (int id, RecService service) =>
            await service.TossAsync(id) ? Results.NoContent() : Results.NotFound());

        // Watchlist
        recs.MapGet("/companies", async (RecService service) => Results.Ok((await service.GetBoardAsync()).Companies));

        recs.MapPost("/companies", async (AddCompanyRequest request, RecService service, CancellationToken cancellationToken) =>
        {
            var (company, error) = await service.AddCompanyAsync(request, cancellationToken);
            return company is null ? Results.BadRequest(error) : Results.Created($"/recs/companies/{company.Id}", company);
        });

        recs.MapDelete("/companies/{id:int}", async (int id, RecService service) =>
            await service.RemoveCompanyAsync(id) ? Results.NoContent() : Results.NotFound());
    }
}
