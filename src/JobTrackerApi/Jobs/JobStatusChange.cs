// One status change in a job's history
public record JobStatusChange(
    int Id,
    int JobId,
    JobStatus? FromStatus, // null when the job was first added
    JobStatus ToStatus,
    DateTime ChangedAt
);
