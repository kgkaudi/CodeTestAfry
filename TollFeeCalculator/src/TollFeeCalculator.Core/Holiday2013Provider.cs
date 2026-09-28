namespace TollFeeCalculator.Core;

/// <summary>
/// Toll-free days for 2013 only: weekends, all of July, and the fixed list of holidays
/// (including the days before holidays) from the original requirements.
/// Dates outside 2013 are rejected instead of silently being treated as ordinary days.
/// </summary>
public sealed class Holiday2013Provider : IHolidayProvider
{
    public const int SupportedYear = 2013;

    private static readonly HashSet<(int Month, int Day)> Holidays = new()
    {
        (1, 1),
        (3, 28), (3, 29),
        (4, 1), (4, 30),
        (5, 1), (5, 8), (5, 9),
        (6, 5), (6, 6), (6, 21),
        (11, 1),
        (12, 24), (12, 25), (12, 26), (12, 31),
    };

    public bool IsTollFreeDay(DateOnly date)
    {
        if (date.Year != SupportedYear)
            throw new ArgumentOutOfRangeException(nameof(date), date,
                $"Only {SupportedYear} is supported.");

        return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
            || date.Month == 7
            || Holidays.Contains((date.Month, date.Day));
    }
}
