namespace BattleBitAPI.Common
{
    public enum Roles : ulong
    {
        None = 0,

        Admin = 1 << 0,
        Moderator = 1 << 1,
        Special = 1 << 2,
        Vip = 1 << 3,
    }
}
