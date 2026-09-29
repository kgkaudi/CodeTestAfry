namespace TollFeeCalculator.Core;

public sealed record DayFee(DateOnly Date, int Passages, int Fee, bool CappedAtMax);

public sealed record TollResult(int TotalFee, IReadOnlyList<DayFee> Days);
