using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using CommunityServerAPI;

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

public class MyPlayer : Player<MyPlayer>
{
    public bool IsAdmin = true;
    public int Kills;
    public int Deaths;
}

class MyGameServer : GameServer<MyPlayer>
{
    public static List<ApiCommand> ApiCommands = new()
    {
        new HelpCommand(),
        new StatsCommand(),
        new KillCommand(),
        new StartCommand()
    };
    
    private CommandHandler handler = new();

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

    public override async Task OnPlayerDisconnected(MyPlayer player)
    {
        await Console.Out.WriteLineAsync("Disconnected: " + player);
    }
    
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<MyPlayer> args)
    {
        if (args.Killer == args.Victim)
        {
            args.Victim.Kill();
            args.Victim.Deaths++;
        }
        else
        {
            args.Victim.Kill();
            args.Killer.SetHP(100);
            args.Killer.Kills++;
            args.Victim.Deaths++;
        }
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        if (player.SteamID == 76561198395073327)
            player.IsAdmin = true;
        
        var splits = msg.Split(" ");
        var cmd = splits[0].ToLower();
        if (!cmd.StartsWith("/")) return true;
        
        foreach(var apiCommand in ApiCommands)
        {
            if (apiCommand.CommandString == cmd || apiCommand.Aliases.Contains(cmd))
            {
                var command = apiCommand.ChatCommand(player, channel, msg);
                if (apiCommand.AdminOnly && !player.IsAdmin)
                    return true;
                    
                await handler.handleCommand(player, command);
                return false;
            }
        }

        return true;
    }
}