using System.Text.Json;
using TollFeeCalculator.Api.Validation;
using TollFeeCalculator.Core;
using Xunit;

namespace TollFeeCalculator.Tests;

public class VehicleTypeParserTests
{
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    [Theory]
    [InlineData("\"Car\"", VehicleType.Car)]
    [InlineData("\"car\"", VehicleType.Car)]         // case-insensitive
    [InlineData("\"CAR\"", VehicleType.Car)]
    [InlineData("\"Motorbike\"", VehicleType.Motorbike)]
    [InlineData("0", VehicleType.Car)]               // numeric enum value
    [InlineData("1", VehicleType.Motorbike)]
    public void Valid_values_are_parsed(string json, VehicleType expected)
    {
        Assert.True(VehicleTypeParser.TryParse(Parse(json), out var result));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("\"Bicycle\"")]  // unknown name
    [InlineData("99")]           // out-of-range number
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void Invalid_values_are_rejected(string json) =>
        Assert.False(VehicleTypeParser.TryParse(Parse(json), out _));

    [Fact]
    public void Missing_field_defaults_to_undefined_and_is_rejected()
    {
        using var doc = JsonDocument.Parse("{}");
        var missing = doc.RootElement.TryGetProperty("vehicleType", out var element) ? element : default;
        Assert.False(VehicleTypeParser.TryParse(missing, out _));
    }
}
