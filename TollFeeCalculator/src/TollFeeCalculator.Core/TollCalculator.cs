namespace TollFeeCalculator.Core;

public sealed class TollCalculator
{
    public const int MaxDailyFee = 60;

    /// <summary>A charge window starts at the first chargeable passage and lasts 60 minutes (end exclusive).</summary>
    public static readonly TimeSpan ChargeWindow = TimeSpan.FromHours(1);

    private readonly FeeSchedule _schedule;
    private readonly IHolidayProvider _holidays;

    public TollCalculator(FeeSchedule schedule, IHolidayProvider holidays)
    {
        _schedule = schedule;
        _holidays = holidays;
    }

    /// <summary>Calculates fees for any number of passages, spanning any number of days, in any order.</summary>
    public TollResult Calculate(VehicleType vehicle, IEnumerable<DateTime> passages)
    {
        ArgumentNullException.ThrowIfNull(passages);

        var days = passages
            .GroupBy(p => DateOnly.FromDateTime(p))
            .OrderBy(g => g.Key)
            .Select(g => CalculateDay(vehicle, g.Key, g.OrderBy(p => p).ToList()))
            .ToList();

        return new TollResult(days.Sum(d => d.Fee), days);
    }

    private DayFee CalculateDay(VehicleType vehicle, DateOnly date, List<DateTime> passages)
    {
        if (vehicle.IsTollFree() || _holidays.IsTollFreeDay(date))
            return new DayFee(date, passages.Count, 0, false);

        // Passages outside charged hours (fee 0) must not open a window and swallow a later paid passage.
        var charged = passages
            .Select(p => (Time: p, Fee: _schedule.GetFee(TimeOnly.FromDateTime(p))))
            .Where(x => x.Fee > 0)
            .ToList();

        int total = 0, i = 0;
        while (i < charged.Count)
        {
            var windowStart = charged[i].Time;
            int highest = 0;
            while (i < charged.Count && charged[i].Time - windowStart < ChargeWindow)
            {
                highest = Math.Max(highest, charged[i].Fee);
                i++;
            }
            total += highest;
        }

        return new DayFee(date, passages.Count, Math.Min(total, MaxDailyFee), total > MaxDailyFee);
    }
}
