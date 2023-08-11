using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        return true;
    }
}
