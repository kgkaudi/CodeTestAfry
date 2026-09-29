using System.Text.Json;
using TollFeeCalculator.Core;

namespace TollFeeCalculator.Api.Validation;

public static class VehicleTypeParser
{
    public static bool TryParse(JsonElement element, out VehicleType result)
    {
        result = default;

        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var name = Enum.GetNames<VehicleType>().FirstOrDefault(n =>
                    string.Equals(n, element.GetString(), StringComparison.OrdinalIgnoreCase));
                if (name is null) return false;
                result = Enum.Parse<VehicleType>(name);
                return true;

            case JsonValueKind.Number when element.TryGetInt32(out var number) && Enum.IsDefined((VehicleType)number):
                result = (VehicleType)number;
                return true;

            default: // missing, null, boolean, object, array, unknown number
                return false;
        }
    }
}