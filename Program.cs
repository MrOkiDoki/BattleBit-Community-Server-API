using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;

internal class Program
{
    private static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(29294);

        Thread.Sleep(-1);
    }
}

internal class MyPlayer : Player<MyPlayer>
{
    public override async Task OnConnected()
    {
    }
}

internal class MyGameServer : GameServer<MyPlayer>
{
    public override async Task OnConnected()
    {
        ForceStartGame();

        ServerSettings.PointLogEnabled = false;
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