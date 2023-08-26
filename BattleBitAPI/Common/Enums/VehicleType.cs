[System.Flags]
public enum VehicleType : byte
{
    None = 0,

    Tank = 1 << 1,
    Transport = 1 << 2,
    SeaVehicle = 1 << 3,
    APC = 1 << 4,
    Helicopters = 1 << 5,

    All = 255,
}