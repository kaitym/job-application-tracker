using Microsoft.EntityFrameworkCore;

// Database connection and tables
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobStatusChange> JobStatusChanges => Set<JobStatusChange>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<ShopProgress> ShopProgress => Set<ShopProgress>();

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
            job.Property(j => j.Description).HasMaxLength(10000);
        });

        // Status history, deleted along with its job
        modelBuilder.Entity<JobStatusChange>(change =>
        {
            change.Property(c => c.FromStatus).HasConversion<string>().HasMaxLength(20);
            change.Property(c => c.ToStatus).HasConversion<string>().HasMaxLength(20);
            change.HasOne<Job>()
                  .WithMany()
                  .HasForeignKey(c => c.JobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Interviews, deleted along with their job
        modelBuilder.Entity<Interview>(interview =>
        {
            interview.Property(i => i.Kind).HasMaxLength(100);
            interview.HasOne<Job>()
                     .WithMany()
                     .HasForeignKey(i => i.JobId)
                     .OnDelete(DeleteBehavior.Cascade);
            interview.HasIndex(i => i.ScheduledAt);
        });

        // One row: how much XP has been collected
        modelBuilder.Entity<ShopProgress>(progress =>
        {
            progress.Property(p => p.Id).ValueGeneratedNever();
        });
    }
}
