// Times rounded to what Postgres stores (microseconds), always UTC
public static class DbTime
{
    public static DateTime UtcNow()
    {
        return Round(DateTime.UtcNow);
    }

    public static DateTime Round(DateTime utc)
    {
        var ticks = utc.Ticks;
        return new DateTime(ticks - ticks % 10, DateTimeKind.Utc);
    }
}
