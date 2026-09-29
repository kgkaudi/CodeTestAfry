using TollFeeCalculator.Core;
using Xunit;

namespace TollFeeCalculator.Tests;

public class FeeScheduleTests
{
    [Theory]
    [InlineData(5, 59, 0)]
    [InlineData(6, 0, 8)]
    [InlineData(6, 29, 8)]
    [InlineData(6, 30, 13)]
    [InlineData(7, 59, 18)]
    [InlineData(8, 0, 13)]
    [InlineData(8, 29, 13)]
    [InlineData(8, 30, 8)]
    [InlineData(9, 15, 8)]   // regression: the original code returned 0 here
    [InlineData(12, 10, 8)]  // regression: same bug
    [InlineData(14, 59, 8)]
    [InlineData(15, 0, 13)]
    [InlineData(15, 29, 13)]
    [InlineData(15, 30, 18)]
    [InlineData(16, 59, 18)]
    [InlineData(17, 0, 13)]
    [InlineData(17, 59, 13)]
    [InlineData(18, 0, 8)]
    [InlineData(18, 29, 8)]
    [InlineData(18, 30, 0)]
    [InlineData(23, 0, 0)]
    public void Default_schedule_returns_expected_fee(int hour, int minute, int expected) =>
        Assert.Equal(expected, FeeSchedule.Default.GetFee(new TimeOnly(hour, minute)));
}
