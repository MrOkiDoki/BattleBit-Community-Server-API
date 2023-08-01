using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitAPI.Storage;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer>();
        listener.OnGameServerTick += OnGameServerTick;
        listener.Start(29294);//Port
        Thread.Sleep(-1);
    }

    private static async Task OnGameServerTick(GameServer server)
    {
        //server.Settings.SpectatorEnabled = !server.Settings.SpectatorEnabled;
        //server.MapRotation.AddToRotation("DustyDew");
        //server.MapRotation.AddToRotation("District");
        //server.GamemodeRotation.AddToRotation("CONQ");
        //server.ForceEndGame();
    }
}
class MyPlayer : Player
{
    
}
