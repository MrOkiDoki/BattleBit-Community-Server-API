using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Common.Enums;
using BattleBitAPI.Server;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer>();
        listener.OnGetPlayerStats += OnGetPlayerStats;
        listener.OnSavePlayerStats += OnSavePlayerStats;
        listener.Start(29294);//Port
        Thread.Sleep(-1);
    }

    public static PlayerStats Stats;

    private static async Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
    {
        Stats = stats;
    }

    private static async Task<PlayerStats> OnGetPlayerStats(ulong steamID)
    {
        if (Stats == null)
            Stats = new PlayerStats();
        Stats.Progress.Rank = 155;
        Stats.Roles = Roles.Moderator;
        Stats.IsBanned = true;
        return Stats;
    }
}
class MyPlayer : Player
{
}
