using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.OnGameServerConnecting += OnGameServerConnecting;
        listener.Start(29294);

        Thread.Sleep(-1);
    }

    private static async Task<bool> OnGameServerConnecting(IPAddress ip)
    {
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
        Console.WriteLine(base.GameIP + " connected");
    }

    public override async Task OnTick()
    {
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
    }

}
