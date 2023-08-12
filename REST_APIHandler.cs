using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommandQueueApp;
using System.Linq;
using System.Threading.Channels;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(55669);
        listener.OnPlayerConnected += HandlePlayerConnected;
        listener.OnTick += HandleTick;
        Thread.Sleep(-1);
    }



    private static async Task<bool> HandleTick()
    {


        return true;
    }

    private static async Task<bool> HandlePlayerConnected(MyPlayer player)
    {
        MyGameServer server = player.GameServer;
        if (!listed_streamers.contains(player.SteamID))
        {
            return true;
        }
        if (server.connected_streamers.contains(player.SteamID))
        {
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

        string[] splits = msg.Split(" ");
        var c = new Command(); // just to test replace with argument parsing for now
        switch (splits[0])
        {
            case "heal":
                {
                    c.Action.set(ActionType.Heal);
                    c.Amount.set(10);
                    c.ExecutorName.set("Tester");
                }
            case "kill":
                {
                    c.Action.set(ActionType.Kill);
                    c.Amount.set(1);
                    c.ExecutorName.set("Tester");
                }
            case "grenade":
                {
                    c.Action.set(ActionType.Grenade);
                    c.Amount.set(1);
                    c.ExecutorName.set("Tester");
                }
            case "teleport":
                {
                    c.Action.set(ActionType.Teleport);
                    c.Amount.set(10);
                    c.ExecutorName.set("Tester");
                }
            case "speed":
                {
                    c.Action.set(ActionType.Speed);
                    c.Amount.set(5);
                    c.ExecutorName.set("Tester");
                }
            case "changeAttachement":
            {
                c.Action.set(ActionType.ChangeAttachement);
                c.Amount.set(1);
                c.Data(splits.Skip(0).Take(splits.Length()));
                c.ExecutorName.set("Tester");
            }
            case "changeWeapon":
            {
                c.Action.set(ActionType.ChangeWeapon);
                c.Amount.set(1);
                c.Data(splits.Skip(0).Take(splits.Length()));
                c.ExecutorName.set("Tester");
            }
            case "reveal":
            {
                c.Action.set(ActionType.Reveal);
                c.Amount.set(10);
                c.ExecutorName.set("Tester");
            }
            case "changeDamage":
            {
                c.Action.set(ActionType.ChangeDamage);
                c.Amount.set(Int32.Parse(splits[1]));
                c.ExecutorName.set("Tester");
            }
            case "changeRecievedDamage":
            {
                c.Action.set(ActionType.ChangeReceivedDamage);
                c.Amount.set(Int32.Parse(splits[1]));
                c.ExecutorName.set("Tester");
            }
            case "changeAmmo":
            {
                c.Action.set(ActionType.ChangeAmmo);
                c.Amount.set(Int32.Parse(splits[1]));
                c.ExecutorName.set("Tester");
            }
                HandleCommand(c);
        }
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
        while (!queue.IsEmpty())
        {
            Command c = queue.Dequeue();
            HandleCommand(c);
        }
    }

    public async Task HandleCommand(Command c)
    {  // need testing if blocking
        foreach (MyPlayer player in connectedStreamers)
        {
            if (player.SteamID != c.StreamerID)
            {
                continue;
            }
            switch (c.Action)
            {
                case ActionType.Heal:
                    {
                        player.Heal(c.Amount);
                        player.Message($"{c.ExecutorName} has healed you for {c.Amount}");
                    }
                case ActionType.Kill:
                    {
                        player.Kill();
                        player.Message($"{c.ExecutorName} has killed you");
                    }
                case ActionType.Grenade:
                    {
                        //can't get player pos right now   
                        player.Message($"{c.ExecutorName} has spawned a grenade on you");
                    }
                case ActionType.Teleport:
                    {
                        //relative teleport????
                        player.Message($"{c.ExecutorName} has teleported you {c.Data}");
                    }
                case ActionType.Speed:
                    {
                        player.setRunningSpeedMultiplyer(c.Amount);
                        player.Message($"{c.ExecutorName} has set your speed to {c.Amount}x");
                    }
                case ActionType.Reveal:
                {
                    //set marker on Map
                    player.Message($"{c.ExecutorName} has revealed your Position");
                }
                case ActionType.ChangeAmmo:
                {
                    //set marker on Map
                    player.Message($"{c.ExecutorName} has set your Ammo to {c.Amount}");
                }

            }

        }
    }
}
