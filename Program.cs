using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommandQueueApp;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text.Json;
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
    public bool isAdmin;
    public bool isStreamer;
}
class MyGameServer : GameServer<MyPlayer>
{
    private readonly string mSteamIdJson = "./config/streamer_steamids.json";
    private readonly string mAdminJson = "./config/admins.json";

    private readonly List<APICommand> ChatCommands = new List<APICommand>()
    {
        new HealCommand(),
        new KillCommand(),
        
    };
    //public CommandQueue queue = new();
    private List<ulong> mListedStreamers = new();
    private List<ulong> mAdmins = new();

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        if (!player.isAdmin)
        {
            return true;
        }
        string[] splits = msg.Split(" ");
        var c = new Command(); // just to test replace with argument parsing for now
        
        switch (splits[0])
        {
            
        }
        await HandleCommand(c);
        return true;
    }

    public List<APICommand> mCommands = new(){ new HealCommand(), new KillCommand(), new TeleportCommand(), new GrenadeCommand()};

    public void SaveStreamers()
    {
        try
        {
            var newJson = JsonSerializer.Serialize(mListedStreamers, new JsonSerializerOptions { WriteIndented = true });

            // Write the JSON to the file, overwriting its content
            File.WriteAllText(mSteamIdJson, newJson);

            Console.WriteLine("Steam IDs updated and saved to the file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Steam IDs couldn't be updated and saved to the file." + ex);
        }
    }
    public override async Task OnConnected()
    {
        await Console.Out.WriteLineAsync(this.GameIP + " Connected");
        await Console.Out.WriteLineAsync("Fetching configs");
        try
        {
            // Read the entire JSON file as a string
            var jsonFilePath = mSteamIdJson;
            var jsonString = File.ReadAllText(jsonFilePath);

            // Parse the JSON array using System.Text.Json
            var steamIds = JsonSerializer.Deserialize<ulong[]>(jsonString);
            foreach (var steamId in steamIds)
            {
                mListedStreamers.Add(steamId);
            }
            await Console.Out.WriteLineAsync("Fetching streamers succeeded");
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Fetching streamers failed: " +ex);
        }
        try
        {
            // Read the entire JSON file as a string
            var jsonFilePath = mAdminJson;
            var jsonString = File.ReadAllText(jsonFilePath);

            // Parse the JSON array using System.Text.Json
            var steamIds = JsonSerializer.Deserialize<ulong[]>(jsonString);
            foreach (var steamId in steamIds)
            {
                mAdmins.Add(steamId);
            }
            await Console.Out.WriteLineAsync("Fetching admins succeeded");
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Fetching admins failed: " +ex);
        }
    }
    public override async Task OnDisconnected()
    {
        await Console.Out.WriteLineAsync(this.GameIP + " Disconnected");
    }

    public override async Task<bool> OnPlayerConnected(MyPlayer player)
    {
        await Console.Out.WriteLineAsync(player.Name + " Connected");
        if (!mListedStreamers.Contains(player.SteamID))
        {
            return true;
        }

        player.isStreamer = true;
        if (!mAdmins.Contains(player.SteamID))
        {
            return true;
        }

        player.isAdmin = true;
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
        foreach (MyPlayer player in AllPlayers)
        {
            if (!player.isStreamer)
            {
                continue;
            }
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
    public class APICommand
    {
        public static string CommandPrefix = String.Empty;
        public static string Help = String.Empty;
        public static Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
        {
            return null;
        }
    }
    public class HealCommand : APICommand
    {
        public static new string CommandPrefix = "!heal";
        public static new string Help = $"{CommandPrefix} 'steamid' 'amount': Heals specific player the specified amount";
        public new static Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
        {
            var splits = msg.Split(" ");
            var c = new Command
            {
                StreamerID = Convert.ToUInt64(splits[1]),
                Action = ActionType.Heal,
                Amount = Int32.Parse(splits[2]),
                ExecutorName = "Chat Test"
            };
            return c;
        }
    }
    public class KillCommand : APICommand
    {
        public static new string CommandPrefix = "!kill";
        public static new string Help = $"{CommandPrefix} 'steamid': Kills specific player";
        public new static Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
        {
            var splits = msg.Split(" ");
            var c = new Command
            {
                StreamerID = Convert.ToUInt64(splits[1]),
                Action = ActionType.Kill,
                Amount = 0,
                ExecutorName = "Chat Test"
            };
            return c;
        }
    }
    public class GrenadeCommand : APICommand
    {
        public static new string CommandPrefix = "!grenade";
        public static new string Help = $"{CommandPrefix} 'steamid': spawns live grenade on specific player";
        public new static Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
        {
            var splits = msg.Split(" ");
            var c = new Command
            {
                StreamerID = Convert.ToUInt64(splits[1]),
                Action = ActionType.Grenade,
                Amount = 0,
                ExecutorName = "Chat Test"
            };
            return c;
        }
    }
    public class TeleportCommand : APICommand
    {
        public static new string CommandPrefix = "!teleport";
        public static new string Help = $"{CommandPrefix} 'steamid' 'vector': tps specific player to vector location";
        public new static Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
        {
            var splits = msg.Split(" ");
            var vectorStr = splits[2].Split(",");
            Vector3 vector = new Vector3()
            {
                X = Convert.ToSingle(vectorStr[0]),
                Y = Convert.ToSingle(vectorStr[1]),
                Z = Convert.ToSingle(vectorStr[1])
            };
            
            var c = new Command
            {
                StreamerID = Convert.ToUInt64(splits[1]),
                Action = ActionType.Teleport,
                Amount = 0,
                Location = vector,
                ExecutorName = "Chat Test"
            };
            return c;
        }
    }
     
    
/*    
    case "!teleport":
                {
                    
                    break;
                }
            case "!speed":
                {
                    c.Action = ActionType.Speed;
                    c.Amount = 5;
                    c.ExecutorName = "Tester";
                    break;
                }
            case "!changeAttachement":
            {
                c.Action = ActionType.ChangeAttachement;
                c.Amount = 1;
                c.Data = splits.Skip(0).Take(splits.Length);
                c.ExecutorName = "Tester";
                break;
            }
            case "!changeWeapon":
            {
                c.Action = ActionType.ChangeWeapon;
                c.Amount = 1;
                c.Data = splits.Skip(0).Take(splits.Length);
                c.ExecutorName = "Tester";
                break;
            }
            case "!reveal":
            {
                c.Action = ActionType.Reveal;
                c.Amount = 10;
                c.ExecutorName = "Tester";
                break;
            }
            case "!changeDamage":
            {
                c.Action = ActionType.ChangeDamage;
                c.Amount = Int32.Parse(splits[1]);
                c.ExecutorName = "Tester";
                break;
            }
            case "!changeRecievedDamage":
            {
                c.Action = ActionType.ChangeReceivedDamage;
                c.Amount = Int32.Parse(splits[1]);
                c.ExecutorName = "Tester";
                break;
            }
            case "!changeAmmo":
            {
                c.Action = ActionType.ChangeAmmo;
                c.Amount = Int32.Parse(splits[2]);
                c.ExecutorName = "Tester";
                break;
            }
            case "!setStreamer":
            {
                var newId = Convert.ToUInt64(splits[1]);
                player.Message($"{newId} is now a Streamer");
                mListedStreamers.Add(newId);
                foreach (var p in AllPlayers)
                {
                    if (p.SteamID == newId)
                    {
                        p.isStreamer = true;
                    }
                }
                SaveStreamers();
                return true;
            }
            case "!rmStreamer":
            {
                var newId = Convert.ToUInt64(splits[1]);
                player.Message($"{newId} is now a Streamer");
                mListedStreamers.Remove(newId);
                foreach (var p in AllPlayers)
                {
                    if (p.SteamID == newId)
                    {
                        p.isStreamer = false;
                    }
                }
                SaveStreamers();
                return true;
            }
            case "!op":
            {
                var newId = Convert.ToUInt64(splits[1]);
                player.Message($"{newId} is now an Admin");
                mAdmins.Add(newId);
                foreach (var p in AllPlayers)
                {
                    if (p.SteamID == newId)
                    {
                        p.isAdmin = true;
                        break;
                    }
                }
                return true;
            }
            case "!deop":
            {
                var newId = Convert.ToUInt64(splits[1]);
                player.Message($"{newId} is no longer an Admin");
                mAdmins.Remove(newId);
                foreach (var p in AllPlayers)
                {
                    if (p.SteamID == newId)
                    {
                        p.isAdmin = false;
                        break;
                    }
                }
                return true;
            }
            case "!start":
            {
                player.Message($"Forcing start");
                ForceStartGame();
                return true;
            }
            case "!help":
            {
                SayToChat("Current commands:");
            }
            */
}
