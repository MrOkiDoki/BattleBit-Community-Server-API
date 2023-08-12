using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommandQueueApp;
using System.Linq;
using System.Threading.Channels;

//a comment
class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(55669);
        Thread.Sleep(-1);
    }




    


}
class MyPlayer : Player<MyPlayer>
{

}
class MyGameServer : GameServer<MyPlayer>
{
    //public CommandQueue queue = new();
    public List<ulong> listed_streamers = new List<ulong>();
    public List<MyPlayer> ConnectedStreamers = new List<MyPlayer>();
    public List<string> ChatMessages = new List<string>();

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {

        string[] splits = msg.Split(" ");
        var c = new Command(); // just to test replace with argument parsing for now
        switch (splits[0])
        {
            case "heal":
                {
                    c.Action = ActionType.Heal;
                    c.Amount = 10;
                    c.ExecutorName = "Tester";
                    break;
                }
            case "kill":
                {
                    c.Action = ActionType.Kill;
                    c.Amount = 1;
                    c.ExecutorName = "Tester";
                    break;
                }
            case "grenade":
                {
                    c.Action = ActionType.Grenade;
                    c.Amount = 1;
                    c.ExecutorName = "Tester";
                    break;
                }
            case "teleport":
                {
                    c.Action = ActionType.Teleport;
                    c.Amount = 10;
                    c.ExecutorName = "Tester";
                    break;
                }
            case "speed":
                {
                    c.Action = ActionType.Speed;
                    c.Amount = 5;
                    c.ExecutorName = "Tester";
                    break;
                }
            case "changeAttachement":
            {
                c.Action = ActionType.ChangeAttachement;
                c.Amount = 1;
                c.Data = splits.Skip(0).Take(splits.Length);
                c.ExecutorName = "Tester";
                break;
            }
            case "changeWeapon":
            {
                c.Action = ActionType.ChangeWeapon;
                c.Amount = 1;
                c.Data = splits.Skip(0).Take(splits.Length);
                c.ExecutorName = "Tester";
                break;
            }
            case "reveal":
            {
                c.Action = ActionType.Reveal;
                c.Amount = 10;
                c.ExecutorName = "Tester";
                break;
            }
            case "changeDamage":
            {
                c.Action = ActionType.ChangeDamage;
                c.Amount = Int32.Parse(splits[1]);
                c.ExecutorName = "Tester";
                break;
            }
            case "changeRecievedDamage":
            {
                c.Action = ActionType.ChangeReceivedDamage;
                c.Amount = Int32.Parse(splits[1]);
                c.ExecutorName = "Tester";
                break;
            }
            case "changeAmmo":
            {
                c.Action = ActionType.ChangeAmmo;
                c.Amount = Int32.Parse(splits[1]);
                c.ExecutorName = "Tester";
                break;
            }
            case "setStreamer":
            {
                listed_streamers.Add(player.SteamID);
                ConnectedStreamers.Add(player);
                return true;
            }
        }
        await HandleCommand(c);
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

    public override async Task<bool> OnPlayerConnected(MyPlayer player)
    {
        await Console.Out.WriteLineAsync(player.Name + " Connected");
        if (!listed_streamers.Contains(player.SteamID))
        {
            return true;
        }
        if (ConnectedStreamers.Contains(player))
        {
            return true;
        }
        ConnectedStreamers.Add(player);
        return true;
    }

    //public override async Task OnTick()
    //{
    //    while (!queue.IsEmpty())
    //    {
    //        Command c = queue.Dequeue();
    //        HandleCommand(c);
    //    }
    //}

    public async Task HandleCommand(Command c)
    {  // need testing if blocking
        foreach (MyPlayer player in ConnectedStreamers)
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
                        break;
                    }
                case ActionType.Kill:
                    {
                        player.Kill();
                        player.Message($"{c.ExecutorName} has killed you");
                        break;
                    }
                case ActionType.Grenade:
                    {
                        //can't get player pos right now   
                        player.Message($"{c.ExecutorName} has spawned a grenade on you");
                        break;
                    }
                case ActionType.Teleport:
                    {
                        //relative teleport????
                        player.Message($"{c.ExecutorName} has teleported you {c.Data}");
                        break;
                    }
                case ActionType.Speed:
                    {
                        player.SetRunningSpeedMultiplier(c.Amount);
                        player.Message($"{c.ExecutorName} has set your speed to {c.Amount}x");
                        break;
                    }
                case ActionType.Reveal:
                {
                    //set marker on Map
                    player.Message($"{c.ExecutorName} has revealed your Position");
                        break;
                }
                case ActionType.ChangeAmmo:
                {
                    //set marker on Map
                    player.Message($"{c.ExecutorName} has set your Ammo to {c.Amount}");
                        break;
                }

            }

        }
    }
}
