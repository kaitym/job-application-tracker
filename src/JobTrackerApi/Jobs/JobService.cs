using Microsoft.EntityFrameworkCore;

// Job logic and database access
public class JobService(AppDbContext db)
{
    // All jobs, oldest first
    public async Task<List<Job>> GetAllAsync()
    {
        return await db.Jobs.OrderBy(j => j.Id).ToListAsync();
    }

    // One job, or null if it doesn't exist
    public async Task<Job?> GetByIdAsync(int id)
    {
        return await db.Jobs.FindAsync(id);
    }

    // Save a new job
    public async Task<Job> CreateAsync(CreateJobRequest request)
    {
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
        return updated;
    }

    // Delete a job, false if it doesn't exist
    public async Task<bool> DeleteAsync(int id)
    {
        var removed = await db.Jobs.Where(j => j.Id == id).ExecuteDeleteAsync();
        return removed > 0;
    }
}
