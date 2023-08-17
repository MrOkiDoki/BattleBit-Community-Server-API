using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Net;
using System.Numerics;
using System.Threading.Channels;
using System.Xml;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.OnGameServerConnecting += OnGameServerConnecting;
        listener.OnValidateGameServerToken += OnValidateGameServerToken;
        listener.Start(29294);

        Thread.Sleep(-1);
    }

    private static async Task<bool> OnValidateGameServerToken(IPAddress ip, ushort gameport, string sentToken)
    {
        return true;
        await Console.Out.WriteLineAsync(ip + ":" + gameport + " sent " + sentToken);
        return sentToken == "12345678910";
    }

    private static async Task<bool> OnGameServerConnecting(IPAddress arg)
    {
        await Console.Out.WriteLineAsync(arg.ToString() + " connecting");
        return true;
    }

}
class MyPlayer : Player<MyPlayer>
{
    public override async Task OnSpawned()
    {
    }
}
class MyGameServer : GameServer<MyPlayer>
{
    public override async Task OnConnected()
    {
        ForceStartGame();
        ServerSettings.PlayerCollision = true;
    }
    public override async Task OnDisconnected()
    {
        await Console.Out.WriteLineAsync("Disconnected: " + this.TerminationReason);
    }

    public override async Task OnTick()
    {

        base.ServerSettings.PlayerCollision = true;
        foreach (var item in AllPlayers)
            item.Modifications.RespawnTime= 0f;
    }

    public override async Task<OnPlayerSpawnArguments> OnPlayerSpawning(MyPlayer player, OnPlayerSpawnArguments request)
    {
        request.Wearings.Eye = "Eye Zombie 01";
        request.Wearings.Face = "Face Zombie 01";
        request.Wearings.Face = "Hair Zombie 01";
        request.Wearings.Skin = "Zombie 01";
        request.Wearings.Uniform = "ANY NU Uniform Zombie 01";
        request.Wearings.Head = "ANV2 Universal Zombie Helmet 00 A Z";
        request.Wearings.Belt = "ANV2_Universal_All_Belt_Null";
        request.Wearings.Backbag = "ANV2_Universal_All_Backpack_Null";
        request.Wearings.Chest = "ANV2_Universal_All_Armor_Null";

        return request;
    }


    public override async Task OnPlayerConnected(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Connected: " + player);

    }
    public override async Task OnPlayerSpawned(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Spawned: " + player);
    }
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<MyPlayer> args)
    {
        await Console.Out.WriteLineAsync("Downed: " + args.Victim);
    }
    public override async Task OnPlayerGivenUp(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Giveup: " + player);
    }
    public override async Task OnPlayerDied(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Died: " + player);
    }
    public override async Task OnAPlayerRevivedAnotherPlayer(MyPlayer from, MyPlayer to)
    {
        await Console.Out.WriteLineAsync(from + " revived " + to);
    }
    public override async Task OnPlayerDisconnected(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Disconnected: " + player);
    }
}
