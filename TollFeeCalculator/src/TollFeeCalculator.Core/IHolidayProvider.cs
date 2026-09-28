namespace TollFeeCalculator.Core;

/// <summary>Decides whether a calendar day is free of toll fees.</summary>
public interface IHolidayProvider
{
    bool IsTollFreeDay(DateOnly date);
}
