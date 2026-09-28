namespace TollFeeCalculator.Core;

public enum VehicleType
{
    Car,
    Motorbike,
    Tractor,
    Emergency,
    Diplomat,
    Foreign,
    Military
}

public static class VehicleTypeExtensions
{
    private static readonly HashSet<VehicleType> TollFree = new()
    {
        VehicleType.Motorbike,
        VehicleType.Tractor,
        VehicleType.Emergency,
        VehicleType.Diplomat,
        VehicleType.Foreign,
        VehicleType.Military
    };

    public static bool IsTollFree(this VehicleType type) => TollFree.Contains(type);
}
