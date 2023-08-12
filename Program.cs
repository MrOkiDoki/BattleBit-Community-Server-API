using System.Text.Json;
using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommunityServerAPI;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Starting API");
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(55669);
        Thread.Sleep(-1);
    }
}

public class MyPlayer : Player<MyPlayer>
{
    private readonly List<Weapon> gunGame = new()
    {
        Weapons.Glock18,
        Weapons.Groza,
        Weapons.ACR,
        Weapons.AK15,
        Weapons.AK74,
        Weapons.G36C,
        Weapons.HoneyBadger,
        Weapons.KrissVector,
        Weapons.L86A1,
        Weapons.L96,
        Weapons.M4A1,
        Weapons.M9,
        Weapons.M110,
        Weapons.M249,
        Weapons.MK14EBR,
        Weapons.MK20,
        Weapons.MP7,
        Weapons.PP2000,
        Weapons.SCARH,
        Weapons.SSG69
    };

    public bool IsAdmin;
    public bool IsStreamer;
    public int Level;

    public void UpdateWeapon()
    {
        SetHeavyGadget("Sledge Hammer", 0);
        if (Level > gunGame.Count) return;
        var w = new WeaponItem
        {
            ToolName = gunGame[Level].Name,
            MainSight = Attachments.RedDot
        };
        SetPrimaryWeapon(w, 10, true);
    }

    public int getGameLenght()
    {
        return gunGame.Count;
    }
}

internal class MyGameServer : GameServer<MyPlayer>
{
    private readonly List<APICommand> ChatCommands = new()
    {
        new HealCommand(),
        new KillCommand(),
        new TeleportCommand(),
        new GrenadeCommand(),
        new ForceStartCommand(),
        new SpeedCommand(),
        new HelpCommand(),
        new RevealCommand(),
        new ChangeDamageCommand(),
        new ChangeReceivedDamageCommand(),
        new ChangeAmmoCommand(),
        new SetStreamerCommand(),
        new RemoveStreamerCommand(),
        new OpCommand(),
        new DeopCommand()
    };


    private readonly string mAdminJson = "./config/admins.json";
    private readonly List<ulong> mAdmins = new();


    //CommandAPI
    //public CommandQueue queue = new();
    private readonly List<ulong> mListedStreamers = new();
    private readonly string mSteamIdJson = "./config/streamer_steamids.json";

    // Gun Game
    public override async Task OnPlayerSpawned(MyPlayer player)
    {
        await Task.Run(() =>
            {
                player.UpdateWeapon();
                player.SetRunningSpeedMultiplier(1.25f);
                player.SetFallDamageMultiplier(0f);
                player.SetJumpMultiplier(1.5f);
            }
        );
    }

    public override async Task<bool> OnAPlayerKilledAnotherPlayer(OnPlayerKillArguments<MyPlayer> onPlayerKillArguments)
    {
        await Task.Run(() =>
        {
            var killer = onPlayerKillArguments.Killer;
            var victim = onPlayerKillArguments.Victim;
            killer.Level++;
            if (killer.Level == killer.getGameLenght()) AnnounceShort($"{killer.Name} only needs 1 more Kill");
            if (killer.Level > killer.getGameLenght())
            {
                AnnounceShort($"{killer.Name} won the Game");
                ForceEndGame();
            }

            if (onPlayerKillArguments.KillerTool == "Sledge Hammer") victim.Level--;
            killer.UpdateWeapon();
        });
        return true;
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        if (!player.IsAdmin) return true;
        var splits = msg.Split(" ");
        foreach (var command in ChatCommands)
            if (splits[0] == command.CommandPrefix)
            {
                var c = command.ChatCommand(player, channel, msg);
                await HandleCommand(c);
                return false;
            }


        return true;
    }


    public void SaveStreamers()
    {
        try
        {
            var newJson =
                JsonSerializer.Serialize(mListedStreamers, new JsonSerializerOptions { WriteIndented = true });

            // Write the JSON to the file, overwriting its content
            File.WriteAllText(mSteamIdJson, newJson);

            Console.WriteLine("Steam IDs updated and saved to the file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Steam IDs couldn't be updated and saved to the file." + ex);
        }
    }

    public void SaveAdmins()
    {
        try
        {
            var newJson =
                JsonSerializer.Serialize(mAdmins, new JsonSerializerOptions { WriteIndented = true });

            // Write the JSON to the file, overwriting its content
            File.WriteAllText(mAdminJson, newJson);

            Console.WriteLine("Admins updated and saved to the file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Admins couldn't be updated and saved to the file." + ex);
        }
    }

    public override async Task OnConnected()
    {
        await Console.Out.WriteLineAsync(GameIP + " Connected");
        await Console.Out.WriteLineAsync("Fetching configs");
        try
        {
            // Read the entire JSON file as a string
            var jsonFilePath = mSteamIdJson;
            var jsonString = File.ReadAllText(jsonFilePath);

            // Parse the JSON array using System.Text.Json
            var steamIds = JsonSerializer.Deserialize<ulong[]>(jsonString);
            foreach (var steamId in steamIds) mListedStreamers.Add(steamId);
            await Console.Out.WriteLineAsync("Fetching streamers succeeded");
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Fetching streamers failed: " + ex);
        }

        try
        {
            // Read the entire JSON file as a string
            var jsonFilePath = mAdminJson;
            var jsonString = File.ReadAllText(jsonFilePath);

            // Parse the JSON array using System.Text.Json
            var steamIds = JsonSerializer.Deserialize<ulong[]>(jsonString);
            foreach (var steamId in steamIds) mAdmins.Add(steamId);
            await Console.Out.WriteLineAsync("Fetching admins succeeded");
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Fetching admins failed: " + ex);
        }
    }


    public override async Task OnDisconnected()
    {
        await Console.Out.WriteLineAsync(GameIP + " Disconnected");
    }


    public override async Task<bool> OnPlayerConnected(MyPlayer player)
    {
        await Console.Out.WriteLineAsync(player.Name + " Connected");
        if (!mListedStreamers.Contains(player.SteamID)) return true;

        player.IsStreamer = true;
        if (!mAdmins.Contains(player.SteamID)) return true;

        player.IsAdmin = true;
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
    {
        // need testing if blocking
        foreach (var player in AllPlayers)
        {
            if (!player.IsStreamer) continue;
            if (player.SteamID != c.StreamerId) continue;
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
                    player.Message($"{c.ExecutorName} has set your Ammo to {c.Amount}");
                    break;
                }
                case ActionType.Start:
                {
                    player.Message("Forcing start");
                    ForceStartGame();
                    break;
                }
                case ActionType.Help:
                {
                    foreach (var command in ChatCommands) SayToChat($"{command.CommandPrefix} {command.Help}");
                    break;
                }
                case ActionType.ChangeDamage:
                {
                    player.SetGiveDamageMultiplier(c.Amount);
                    player.Message($"{c.ExecutorName} has set your Dmg Multiplier to {c.Amount}");
                    break;
                }
                case ActionType.ChangeReceivedDamage:
                {
                    player.SetReceiveDamageMultiplier(c.Amount);
                    player.Message($"{c.ExecutorName} has set your receive Dmg Multiplier to {c.Amount}");
                    break;
                }
                case ActionType.SetStreamer:
                {
                    player.IsStreamer = true;
                    SaveStreamers();
                    break;
                }
                case ActionType.RemoveStreamer:
                {
                    player.IsStreamer = false;
                    SaveStreamers();
                    break;
                }
                case ActionType.GrantOP:
                {
                    player.IsAdmin = true;
                    SaveAdmins();
                    break;
                }
                case ActionType.RevokeOP:
                {
                    player.IsAdmin = false;
                    SaveAdmins();
                    break;
                }
                // Add more cases for other ActionType values as needed
            }
        }
    }
}