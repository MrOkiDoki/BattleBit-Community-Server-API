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
        listener.Start(30001);
        
        Console.WriteLine("API started!");

        Thread.Sleep(-1);
    }


}
class MyPlayer : Player<MyPlayer>
{
    public int Kills;
    public int Deaths;
    public List<MyPlayer> players;
}
class MyGameServer : GameServer<MyPlayer>
{
    private List<MyPlayer> players;
    
    public override async Task OnConnected()
    {
        Console.WriteLine($"Gameserver connected! {this.GameIP}:{this.GamePort}");

        ServerSettings.BleedingEnabled = false;
        ServerSettings.SpectatorEnabled = false;
    }
    
    public override async Task OnDisconnected()
    {
        Console.WriteLine($"Gameserver disconnected! {this.GameIP}:{this.GamePort}");
    }

    public override async Task OnReconnected()
    {
        Console.WriteLine($"Gameserver reconnected! {this.GameIP}:{this.GamePort}");
        
        ServerSettings.BleedingEnabled = false;
        ServerSettings.SpectatorEnabled = false;
    }

    public override async Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        await Console.Out.WriteLineAsync("Giveup: " + player);
    }
    public override async Task OnPlayerDied(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Died: " + player);
    }
    public override async Task OnAPlayerRevivedAnotherPlayer(MyPlayer from, MyPlayer to)
    {
        await Console.Out.WriteLineAsync(from + " revived " + to);
    }
    public override async Task OnPlayerDisconnected(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Disconnected: " + player);
    }

    public override async Task OnTick()
    {
        var top5 = players.OrderByDescending(x => x.Kills / x.Deaths).Take(5).ToList();
        var topPlayersInfo = top5.Select((p, index) => $"{index + 1}. {p.Name} - {p.Kills / p.Deaths}");
        var announcement = $"<align=\"center\">--- Top 5 Players ---\n{string.Join("\n", topPlayersInfo)}</align>";
        
        AnnounceShort(announcement);
    }
    
    private MyPlayer FindPlayerByIdentifier(string identifier)
    {
        ulong.TryParse(identifier, out ulong steamid);
        return players.FirstOrDefault(x => x.SteamID == steamid || x.Name == identifier);
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        if (player.SteamID != 76561198395073327 || !msg.StartsWith("/")) return true;

        var words = msg.Split(" ");
        var type = ulong.TryParse(words[1], out _) ? "steamid" : "name";

        switch (words[0])
        {
            case "/tp":
                // /tp <steamid/name>
                var tpTarget = FindPlayerByIdentifier(words[1]);
                
                if (tpTarget == null)
                {
                    player.Message("Player not found!");
                    return false;
                }
                
                player.Message("Not implemented yet!");
                break;
            
            case "/kill":
                // /kill <steamid/name>
                var killTarget = FindPlayerByIdentifier(words[1]);
                
                if (killTarget == null)
                {
                    player.Message("Player not found!");
                    return false;
                }
                
                killTarget.Kill();
                break;
            
            case "/heal":
                // /heal <steamid/name> <amount>
                var healTarget = FindPlayerByIdentifier(words[1]);
                
                if (healTarget == null)
                {
                    player.Message("Player not found!");
                    return false;
                }
                
                var healAmount = words.Length > 2 ? int.Parse(words[2]) : 100;
                healTarget.Heal(healAmount);
                break;
            
            case "/kick":
                // /kick <steamid/name> <reason>
                var kickTarget = FindPlayerByIdentifier(words[1]);
                
                if (kickTarget == null)
                {
                    player.Message("Player not found!");
                    return false;
                }
                
                var kickReason = words.Length > 2 ? string.Join(" ", words.Skip(2)) : "No reason provided";
                
                kickTarget.Kick(kickReason);
                break;
            default:
                player.Message("Unknown command!");
                break;
        }
        
        return false;
    }

    public override async Task OnRoundStarted()
    {
        players = AllPlayers.ToList();

        foreach (var player in players)
        {
            player.Kills = 0;
            player.Deaths = 0;
        }
    }


    public override async Task OnPlayerConnected(MyPlayer player)
    {
        player.Kills = 0;
        player.Deaths = 0;
    }

    public override Task<PlayerStats> OnGetPlayerStats(ulong steamID, PlayerStats officialStats)
    {
        var stats = new PlayerStats();

        stats.Progress.Rank = 200;
        stats.Progress.Prestige = 10;
        stats.Progress.EXP = uint.MaxValue;
        if (steamID == 76561198395073327)
        {
            stats.Roles = Roles.Admin;
        }
        
        return Task.FromResult(stats);
    }
    public override async Task OnAPlayerKilledAnotherPlayer(OnPlayerKillArguments<MyPlayer> args)
    {
        args.Killer.Heal(100);
        args.Killer.Kills++;
        args.Victim.Deaths++;
    }

    public override async Task <bool> OnPlayerRequestingToChangeRole(MyPlayer player, GameRole role)
    {
        if (role == GameRole.Assault)
            return true;
        
        player.GameServer.AnnounceShort("You can only play as Assault!");
        return false;
    }

    public override async Task<OnPlayerSpawnArguments> OnPlayerSpawning(MyPlayer player, OnPlayerSpawnArguments request)
    {
        request.Loadout.PrimaryWeapon = default;
        request.Loadout.SecondaryWeapon = default;
        request.Loadout.LightGadget = null;
        request.Loadout.HeavyGadget = Gadgets.AdvancedBinoculars;
        request.Loadout.Throwable = null;
        request.Loadout.FirstAid = null;
        request.Loadout.PrimaryExtraMagazines = 5;
        request.Loadout.SecondaryExtraMagazines = 5;

        return request;
    }
    
    public override async Task OnPlayerSpawned(MyPlayer player)
    {
        player.SetGiveDamageMultiplier(0.25f);
    }
}
