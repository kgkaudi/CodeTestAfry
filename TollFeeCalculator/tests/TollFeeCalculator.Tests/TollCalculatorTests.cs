using TollFeeCalculator.Core;
using Xunit;

namespace TollFeeCalculator.Tests;

public class TollCalculatorTests
{
    private readonly TollCalculator _sut = new(FeeSchedule.Default, new Holiday2013Provider());

    // Monday 2013-03-11 and Tuesday 2013-03-12 are ordinary working days.
    private static DateTime Mon(int h, int m) => new(2013, 3, 11, h, m, 0);
    private static DateTime Tue(int h, int m) => new(2013, 3, 12, h, m, 0);

    private int Total(VehicleType v, params DateTime[] p) => _sut.Calculate(v, p).TotalFee;

    [Fact]
    public void Single_passage_is_charged() =>
        Assert.Equal(18, Total(VehicleType.Car, Mon(7, 30)));

    [Fact]
    public void No_passages_costs_nothing() =>
        Assert.Equal(0, Total(VehicleType.Car));

    [Theory]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Tractor)]
    [InlineData(VehicleType.Emergency)]
    [InlineData(VehicleType.Diplomat)]
    [InlineData(VehicleType.Foreign)]
    [InlineData(VehicleType.Military)]
    public void Exempt_vehicles_are_free(VehicleType type) =>
        Assert.Equal(0, Total(type, Mon(7, 30)));

    [Fact]
    public void Weekend_is_free() =>
        Assert.Equal(0, Total(VehicleType.Car, new DateTime(2013, 3, 16, 7, 30, 0)));

    [Fact]
    public void July_is_free() =>
        Assert.Equal(0, Total(VehicleType.Car, new DateTime(2013, 7, 1, 7, 30, 0)));

    [Fact]
    public void Highest_fee_within_the_hour_applies() =>
        Assert.Equal(13, Total(VehicleType.Car, Mon(6, 0), Mon(6, 35)));

    [Fact]
    public void Window_is_anchored_on_first_passage_not_the_latest()
    {
        // 06:00 opens a window; 06:45 is inside it; 07:05 is 65 min after 06:00 => new window.
        Assert.Equal(13 + 18, Total(VehicleType.Car, Mon(6, 0), Mon(6, 45), Mon(7, 5)));
    }

    [Fact]
    public void Exactly_sixty_minutes_later_starts_a_new_window() =>
        Assert.Equal(8 + 18, Total(VehicleType.Car, Mon(6, 0), Mon(7, 0)));

    [Fact]
    public void Unpaid_early_passage_does_not_swallow_a_paid_one() =>
        Assert.Equal(8, Total(VehicleType.Car, Mon(5, 50), Mon(6, 20)));

    [Fact]
    public void Input_order_does_not_matter()
    {
        var asc = new[] { Mon(6, 0), Mon(6, 45), Mon(7, 5) };
        var desc = asc.Reverse().ToArray();
        Assert.Equal(Total(VehicleType.Car, asc), Total(VehicleType.Car, desc));
    }

    [Fact]
    public void Daily_fee_is_capped_at_60()
    {
        // 18 + 13 + 8 + 18 + 13 = 70 uncapped
        var result = _sut.Calculate(VehicleType.Car,
            new[] { Mon(7, 0), Mon(8, 0), Mon(9, 0), Mon(15, 30), Mon(17, 0) });

        Assert.Equal(60, result.TotalFee);
        Assert.True(result.Days.Single().CappedAtMax);
    }

    [Fact]
    public void Cap_applies_per_day_and_days_are_summed()
    {
        var result = _sut.Calculate(VehicleType.Car, new[] { Mon(7, 30), Tue(7, 30) });

        Assert.Equal(36, result.TotalFee);
        Assert.Equal(2, result.Days.Count);
    }

    [Fact]
    public void Passages_outside_2013_are_rejected() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.Calculate(VehicleType.Car, new[] { new DateTime(2014, 3, 11, 7, 30, 0) }));
}
