using TollFeeCalculator.Core;
using Xunit;

namespace TollFeeCalculator.Tests;

public class HolidayProviderTests
{
    private readonly Holiday2013Provider _sut = new();

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 16)]  // Saturday
    [InlineData(3, 17)]  // Sunday
    [InlineData(3, 28)]
    [InlineData(3, 29)]
    [InlineData(4, 1)]
    [InlineData(4, 30)]
    [InlineData(5, 1)]
    [InlineData(5, 8)]
    [InlineData(5, 9)]
    [InlineData(6, 5)]
    [InlineData(6, 6)]
    [InlineData(6, 21)]
    [InlineData(7, 1)]
    [InlineData(7, 31)]
    [InlineData(11, 1)]
    [InlineData(12, 24)]
    [InlineData(12, 25)]
    [InlineData(12, 26)]
    [InlineData(12, 31)]
    public void Toll_free_days(int month, int day) =>
        Assert.True(_sut.IsTollFreeDay(new DateOnly(2013, month, day)));

    [Theory]
    [InlineData(3, 11)]
    [InlineData(3, 27)]
    [InlineData(4, 2)]
    [InlineData(8, 1)]
    [InlineData(12, 23)]
    public void Ordinary_working_days_are_not_toll_free(int month, int day) =>
        Assert.False(_sut.IsTollFreeDay(new DateOnly(2013, month, day)));

    [Theory]
    [InlineData(2012)]
    [InlineData(2014)]
    public void Other_years_are_rejected(int year) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.IsTollFreeDay(new DateOnly(year, 3, 11)));
}
