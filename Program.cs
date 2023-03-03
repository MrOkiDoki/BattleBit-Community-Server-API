using BattleBitAPI.Server;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        ServerListener server = new ServerListener();
        server.OnGameServerConnecting += OnClientConnecting;
        server.OnGameServerConnected += OnGameServerConnected;
        server.OnGameServerDisconnected += OnGameServerDisconnected;

        server.Start(29294);
    }

    private static async Task<bool> OnClientConnecting(IPAddress ip)
    {
        return true;
    }
    private static async Task OnGameServerConnected(GameServer server)
    {
        Console.WriteLine("Server "+server.ServerName+" was connected.");
    }
    private static async Task OnGameServerDisconnected(GameServer server)
    {
        Console.WriteLine("Server " + server.ServerName + " was disconnected.");
    }
}