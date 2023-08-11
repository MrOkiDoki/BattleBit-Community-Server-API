using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Threading.Channels;

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
    public List<string> ChatMessages = new List<string>();

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        return true;
    }
    public override async Task OnConnected()
    {
        await Console.Out.WriteLineAsync(this.GameIP + " Connected");
    }
    public override async Task OnDisconnected()
    {
        await Console.Out.WriteLineAsync(this.GameIP + " Disconnected");
    }

    public override async Task OnTick()
    {
        if (RoundSettings.State == GameState.WaitingForPlayers)
        {
            int numberOfPeopleInServer = this.CurrentPlayers;
            if (numberOfPeopleInServer > 4)
            {
                ForceStartGame();
            }
        }
        else if (RoundSettings.State == GameState.Playing)
        {

        }
    }
}
