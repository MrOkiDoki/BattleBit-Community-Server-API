namespace BattleBitAPI.Common
{
    [System.Flags]
    public enum SpawningRule : ulong
    {
        None = 0,

        Flags = 1 << 0,
        SquadMates = 1 << 1,
        SquadCaptain = 1 << 2,

        Tanks = 1 << 3,
        Transports = 1 << 4,
        Boats = 1 << 5,
        Helicopters = 1 << 6,
        APCs = 1 << 7,

        RallyPoints = 1 << 8,

        All = ulong.MaxValue,
    }
}
