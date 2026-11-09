namespace ClearPay.Reconciliation;

/// <summary>
/// Settlement days are Istanbul calendar days, not UTC days: a capture at 23:30 Istanbul (20:30 UTC)
/// belongs to that day's file. Turkey has no DST since 2016, but the code still asks the time zone
/// database instead of hard-coding +03:00.
/// </summary>
public static class BusinessDay
{
    public const string TimeZoneId = "Europe/Istanbul";

    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    public static DateOnly Of(DateTimeOffset instant) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, TimeZone).DateTime);

    /// <summary>The day before "today in Istanbul": what the 02:00 job reconciles.</summary>
    public static DateOnly Previous(DateTimeOffset now) => Of(now).AddDays(-1);

    /// <summary>[start, end) of the day as instants, for queries on datetimeoffset columns (the ledger SP).</summary>
    public static (DateTimeOffset Start, DateTimeOffset End) Window(DateOnly day) => (StartOf(day), StartOf(day.AddDays(1)));

    private static DateTimeOffset StartOf(DateOnly day)
    {
        var local = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, TimeZone.GetUtcOffset(local));
    }
}
