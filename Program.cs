using BattleBitAPI;
using BattleBitAPI.Server;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(29294);
        listener.OnGameServerConnecting += OnGameServerConnecting;

        Thread.Sleep(-1);
    }

    private static async Task<bool> OnGameServerConnecting(IPAddress ip)
    {
        return true;
    }
}
class MyPlayer : Player<MyPlayer>
{
    public int NumberOfSpawns = 0;

    public override async Task OnSpawned()
    {
        this.NumberOfSpawns++;
        base.GameServer.CloseConnection();

        await Console.Out.WriteLineAsync("Spawn: " + this.NumberOfSpawns);
    }
}
class MyGameServer : GameServer<MyPlayer>
{
    public override async Task OnConnected()
    {
        Console.WriteLine(base.GameIP + " connected");
    }
}
