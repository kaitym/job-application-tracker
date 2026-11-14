using Microsoft.EntityFrameworkCore;

// Stats and XP, calculated from jobs and their history
public class StatsService(AppDbContext db)
{
    private static readonly JobStatus[] ResponseStatuses =
        [JobStatus.Interviewing, JobStatus.Offer, JobStatus.Rejected];

    public async Task<StatsResponse> GetAsync()
    {
        // Count jobs per status (SQL GROUP BY)
        var counts = await db.Jobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var byStatus = Enum.GetValues<JobStatus>().ToDictionary(
            status => status,
            status => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0);

        // Every status each job has ever reached, once each
        var reached = await db.JobStatusChanges
            .Select(c => new { c.JobId, c.ToStatus })
            .Distinct()
            .ToListAsync();

        var respondedIds = await db.Jobs
            .Where(j => j.RespondedAt != null)
            .Select(j => j.Id)
            .ToListAsync();

        var applications = reached
            .Where(r => r.ToStatus != JobStatus.Saved)
            .Select(r => r.JobId)
            .Distinct()
            .Count();

        var responses = reached
            .Where(r => ResponseStatuses.Contains(r.ToStatus))
            .Select(r => r.JobId)
            .Concat(respondedIds)
            .Distinct()
            .Count();

        // XP: first time reaching each status, plus each response
        var xp = reached.Sum(r => Leveling.XpForReaching(r.ToStatus))
                 + responses * Leveling.ResponseXp;

        // Level only goes up when XP is collected
        var collected = await GetCollectedXpAsync();
        var now = DbTime.UtcNow();

        return new StatsResponse(
            TotalJobs: byStatus.Values.Sum(),
            ByStatus: byStatus,
            Applications: applications,
            Responses: responses,
            ResponseRate: applications == 0 ? 0 : Math.Round((double)responses / applications, 2),
            NeedsAction: await db.Jobs.CountAsync(j => j.NeedsAction),
            UpcomingInterviews: await db.Interviews.CountAsync(i => i.ScheduledAt >= now),
            Xp: xp,
            CollectedXp: collected,
            PendingXp: Math.Max(0, xp - collected),
            Level: Leveling.LevelFor(collected),
            LevelStartXp: Leveling.LevelThresholds[Leveling.LevelFor(collected) - 1],
            NextLevelXp: Leveling.XpForNextLevel(collected)
        );
    }

    // Move all earned XP into the shop
    public async Task<CollectResponse> CollectAsync()
    {
        var before = await GetAsync();

        var progress = await db.ShopProgress.FindAsync(1);
        var collected = Math.Max(before.CollectedXp, before.Xp);

        if (progress is null)
        {
            db.ShopProgress.Add(new ShopProgress(1, collected));
        }
        else
        {
            db.Entry(progress).CurrentValues.SetValues(progress with { CollectedXp = collected });
        }

        await db.SaveChangesAsync();

        var after = await GetAsync();
        return new CollectResponse(after.CollectedXp - before.CollectedXp, before.Level, after);
    }

    private async Task<int> GetCollectedXpAsync()
    {
        var progress = await db.ShopProgress.AsNoTracking().FirstOrDefaultAsync(p => p.Id == 1);
        return progress?.CollectedXp ?? 0;
    }
}
