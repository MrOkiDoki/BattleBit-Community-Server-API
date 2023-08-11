using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommandQueueApp;
using System.Threading.Channels;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(29294);
        listener.OnPlayerConnected += HandlePlayerConnected;
        listener.OnTick += HandleTick;
        Thread.Sleep(-1);
    }



    private static async Task<bool> HandleTick(){

        
        return true;
    }

    private static async Task<bool> HandlePlayerConnected(MyPlayer player)
    {
        MyGameServer server = player.GameServer;
        if(!listed_streamers.contains(player.SteamID)){
            return true;
        }
        if(server.connected_streamers.contains(player.SteamID)){
            return true;
        }
        server.connected_streamers.Add(player);
    }


}
class MyPlayer : Player<MyPlayer>
{

}
class MyGameServer : GameServer<MyPlayer>
{
    public CommandQueue queue = new();
    public List<MyPlayer> connectedStreamers = new List<MyPlayer>();
    public List<string> ChatMessages = new List<string>();

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        var c = new Command(); // just to test
        c.Action.set("heal");
        c.Amount.set(10);
        c.ExecuterName.set("Tester");
        HandleCommand(c);
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
        while(!queue.IsEmpty()){
            Command c = queue.Dequeue();
            HandleCommand(c);
        }
    }

    public async Task HandleCommand(Command c){  // need testing if blocking
        foreach(MyPlayer player in connectedStreamers){
            if(player.SteamID != c.StreamerID){
                continue;
            }
            switch (c.Action)
            {
            case "heal":{
                player.Heal(c.Amount);
                player.Message($"{c.ExecuterName} has healed you for {c.Amount}");
            }
            case "kill":{
                player.Kill();
                player.Message($"{c.ExecuterName} has killed you");
            }
            case "grenade":{
                //can't get player pos right now   
                player.Message($"{c.ExecuterName} has spawned a grenade on you");
            }
            case "teleport":{
                //relative teleport????
                player.Message($"{c.ExecuterName} has teleported you {c.Data}");
            }
            case "speed":{
                player.setRunningSpeedMultiplyer(c.Amount);
                player.Message($"{c.ExecuterName} has set your speed to {c.Amount}x");
            }

            }
            
        }
    }
}
