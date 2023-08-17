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
    private readonly List<ApiCommand> mApiCommands = new()
    {
        new KillCommand(),
        new StartCommand()
    };

    private CommandHandler handler = new();
    
    public List<MyPlayer> Players;

    public override async Task OnConnected()
    {
        Console.WriteLine($"Gameserver connected! {this.GameIP}:{this.GamePort}");
        Console.Write(mApiCommands.Count);

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

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        if (player.IsAdmin)
        {
            var splits = msg.Split(" ");
            var cmd = splits[0].ToLower();
            foreach(var apiCommand in mApiCommands)
            {
                if (apiCommand.CommandString == cmd)
                {
                    var command = apiCommand.ChatCommand(player, channel, msg);
                    await handler.handleCommand(player, command);
                    return false;
                }
            }
        }

        return true;
    }
}