// What I did with a rec
public enum RecState
{
    New,
    Added,
    Tossed
}

// A job found on a watched company's board
public record JobRec(
    int Id,
    int WatchedCompanyId,
    string ExternalId, // the board's id for the posting
    string Company,
    string Title,
    string Location,
    string Url,
    DateTime? PostedAt,
    DateTime FoundAt,
    int Priority, // 3 = Orange County, 2 = San Diego, 1 = SF / remote
    RecState State,
    int? JobId // the job it became, once added to the menu
);
