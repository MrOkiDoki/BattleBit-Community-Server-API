using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Threading.Channels;
using System.Xml;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(29294);

        Thread.Sleep(-1);
    }


}
class MyPlayer : Player<MyPlayer>
{
}
class MyGameServer : GameServer<MyPlayer>
{
    public override async Task OnConnected()
    {
        ForceStartGame();
    }
    public override async Task OnTick()
    {
        foreach (var player in AllPlayers)
        {
            await Console.Out.WriteLineAsync(player + " : " + player.HP + " : " + player.Position);
        }
    }
}
