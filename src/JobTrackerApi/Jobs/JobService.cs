using Microsoft.EntityFrameworkCore;

// Job logic and database access
public class JobService(AppDbContext db)
{
    // Jobs matching the filters, oldest first
    public async Task<List<Job>> GetAllAsync(JobStatus? status = null, string? search = null, bool? needsAction = null)
    {
        var query = db.Jobs.AsQueryable();

        if (status is not null)
        {
            query = query.Where(j => j.Status == status);
        }

        if (needsAction is not null)
        {
            query = query.Where(j => j.NeedsAction == needsAction);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Case-insensitive match on company, role, or location
            var pattern = $"%{search.Trim()}%";
            query = query.Where(j =>
                EF.Functions.ILike(j.Company, pattern) ||
                EF.Functions.ILike(j.Role, pattern) ||
                EF.Functions.ILike(j.Location, pattern));
        }

        return await query.OrderBy(j => j.Id).ToListAsync();
    }

    // One job, or null if it doesn't exist
    public async Task<Job?> GetByIdAsync(int id)
    {
        return await db.Jobs.FindAsync(id);
    }

    // Save a new job and its first history entry
    public async Task<Job> CreateAsync(CreateJobRequest request)
    {
        var now = UtcNow();

        var job = new Job(
            0,
            request.Company,
            request.Role,
            request.Location,
            request.Status,
            request.DateApplied,
            request.Link,
            request.Notes,
            request.Deadline,
            request.NeedsAction,
            request.NeedsAction ? now : null
        );

        await using var transaction = await db.Database.BeginTransactionAsync();

        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        db.JobStatusChanges.Add(new JobStatusChange(0, job.Id, null, job.Status, now));
        await db.SaveChangesAsync();

        await transaction.CommitAsync();
        return job;
    }

    // Update a job, or null if it doesn't exist
    public async Task<Job?> UpdateAsync(int id, UpdateJobRequest request)
    {
        var existing = await db.Jobs.FindAsync(id);
        if (existing is null)
        {
            return null;
        }

        var now = UtcNow();
        var oldStatus = existing.Status;

        // Remember the first time they responded
        var respondedAt = existing.RespondedAt ?? (request.NeedsAction ? now : null);

        var updated = existing with
        {
            Company = request.Company,
            Role = request.Role,
            Location = request.Location,
            Status = request.Status,
            DateApplied = request.DateApplied,
            Link = request.Link,
            Notes = request.Notes,
            Deadline = request.Deadline,
            NeedsAction = request.NeedsAction,
            RespondedAt = respondedAt
        };

        db.Entry(existing).CurrentValues.SetValues(updated);

        if (updated.Status != oldStatus)
        {
            db.JobStatusChanges.Add(new JobStatusChange(0, id, oldStatus, updated.Status, now));
        }

        await db.SaveChangesAsync();
        return updated;
    }

    // Delete a job (and its history), false if it doesn't exist
    public async Task<bool> DeleteAsync(int id)
    {
        var removed = await db.Jobs.Where(j => j.Id == id).ExecuteDeleteAsync();
        return removed > 0;
    }

    // A job's status changes, oldest first, or null if the job doesn't exist
    public async Task<List<JobStatusChange>?> GetHistoryAsync(int id)
    {
        if (!await db.Jobs.AnyAsync(j => j.Id == id))
        {
            return null;
        }

        return await db.JobStatusChanges
            .Where(c => c.JobId == id)
            .OrderBy(c => c.ChangedAt)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    // Applied jobs with no news for a while ("gone cold")
    public async Task<List<Job>> GetGoneColdAsync(int days)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        return await db.Jobs
            .Where(j => j.Status == JobStatus.Applied && !j.NeedsAction)
            .Where(j => db.JobStatusChanges
                .Where(c => c.JobId == j.Id)
                .Max(c => c.ChangedAt) < cutoff)
            .OrderBy(j => j.Id)
            .ToListAsync();
    }

    // Current time, rounded to what Postgres stores (microseconds)
    private static DateTime UtcNow()
    {
        var ticks = DateTime.UtcNow.Ticks;
        return new DateTime(ticks - ticks % 10, DateTimeKind.Utc);
    }
}
