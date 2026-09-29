using System.Text.Json;

namespace TollFeeCalculator.Api.Models;

/// <summary>
/// VehicleType is read as raw JSON so that every invalid value (unknown name, wrong JSON type,
/// out-of-range number, or a missing field) gets a validation message instead of an empty 400.
/// </summary>
public sealed record CalculateRequest(JsonElement VehicleType, List<DateTime>? Passages);