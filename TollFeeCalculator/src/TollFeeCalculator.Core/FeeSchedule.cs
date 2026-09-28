namespace TollFeeCalculator.Core;

/// <summary>A fee applying from <c>From</c> (inclusive) to <c>To</c> (exclusive).</summary>
public sealed record FeeBand(TimeOnly From, TimeOnly To, int Fee);

public sealed class FeeSchedule
{
    private static TimeOnly T(int h, int m) => new(h, m);

    public static FeeSchedule Default { get; } = new(new[]
    {
        new FeeBand(T(6, 0),  T(6, 30),  8),
        new FeeBand(T(6, 30), T(7, 0),  13),
        new FeeBand(T(7, 0),  T(8, 0),  18),
        new FeeBand(T(8, 0),  T(8, 30), 13),
        new FeeBand(T(8, 30), T(15, 0),  8),
        new FeeBand(T(15, 0), T(15, 30), 13),
        new FeeBand(T(15, 30), T(17, 0), 18),
        new FeeBand(T(17, 0), T(18, 0), 13),
        new FeeBand(T(18, 0), T(18, 30), 8),
    });

    private readonly IReadOnlyList<FeeBand> _bands;

    public FeeSchedule(IEnumerable<FeeBand> bands) => _bands = bands.ToList();

    public int GetFee(TimeOnly time) =>
        _bands.FirstOrDefault(b => time >= b.From && time < b.To)?.Fee ?? 0;
}
