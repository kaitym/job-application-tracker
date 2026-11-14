using Microsoft.EntityFrameworkCore;

// Interview logic and database access
public class InterviewService(AppDbContext db)
{
    // A job's interviews, soonest first, or null if the job doesn't exist
    public async Task<List<Interview>?> GetForJobAsync(int jobId)
    {
        if (!await db.Jobs.AnyAsync(j => j.Id == jobId))
        {
            return null;
        }

        return await db.Interviews
            .Where(i => i.JobId == jobId)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync();
    }

    // Schedule an interview, or null if the job doesn't exist
    public async Task<Interview?> CreateAsync(int jobId, CreateInterviewRequest request)
    {
        if (!await db.Jobs.AnyAsync(j => j.Id == jobId))
        {
            return null;
        }

        var interview = new Interview(
            0,
            jobId,
            DbTime.Round(request.ScheduledAt.UtcDateTime),
            request.Kind,
            request.Notes
        );

        db.Interviews.Add(interview);
        await db.SaveChangesAsync();
        return interview;
    }

    // Delete an interview, false if it doesn't exist on this job
    public async Task<bool> DeleteAsync(int jobId, int interviewId)
    {
        var removed = await db.Interviews
            .Where(i => i.Id == interviewId && i.JobId == jobId)
            .ExecuteDeleteAsync();
        return removed > 0;
    }

    // Interviews in the next few days, with company and role
    public async Task<List<UpcomingInterview>> GetUpcomingAsync(int days)
    {
        var now = DbTime.UtcNow();
        var until = now.AddDays(days);

        return await db.Interviews
            .Where(i => i.ScheduledAt >= now && i.ScheduledAt <= until)
            .OrderBy(i => i.ScheduledAt)
            .Join(db.Jobs,
                interview => interview.JobId,
                job => job.Id,
                (interview, job) => new UpcomingInterview(
                    interview.Id,
                    job.Id,
                    job.Company,
                    job.Role,
                    interview.ScheduledAt,
                    interview.Kind,
                    interview.Notes))
            .ToListAsync();
    }
}
