using Microsoft.EntityFrameworkCore;

// Database connection and tables
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Column rules for the jobs table
        modelBuilder.Entity<Job>(job =>
        {
            job.Property(j => j.Company).HasMaxLength(200);
            job.Property(j => j.Role).HasMaxLength(200);
            job.Property(j => j.Location).HasMaxLength(200);
            job.Property(j => j.Status).HasConversion<string>().HasMaxLength(20);
            job.Property(j => j.Link).HasMaxLength(500);
        });
    }
}
