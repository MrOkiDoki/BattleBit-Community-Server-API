using System.Numerics;
using BattleBitAPI;
using CommunityServerAPI.Enums;

namespace CommunityServerAPI;

public class Command
{
    public ActionType Action { get; set; }
    public ulong SteamId { get; set; }
    public string Executor { get; set; }
    public Player<MyPlayer> Target { get; set; }
    public ulong TargetSteamId { get; set; }
    public string Message { get; set; }
    public Vector3 Location { get; set; }
    public int Amount { get; set; }
    public int Duration { get; set; }
    public string Reason { get; set; }
    public bool Error { get; set; }
}