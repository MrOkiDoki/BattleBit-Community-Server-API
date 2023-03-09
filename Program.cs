#region

using System.Net;

using BattleBitAPI.Client;
using BattleBitAPI.Server;

#endregion

internal class Program
{
	private static void Main(string[] args)
	{
		if (Console.ReadLine().Contains("h"))
		{
			var server = new ServerListener();
			server.OnGameServerConnecting += OnClientConnecting;
			server.OnGameServerConnected += OnGameServerConnected;
			server.OnGameServerDisconnected += OnGameServerDisconnected;
			server.Start(29294);

			Thread.Sleep(-1);
		}
		else
		{
			var c = new Client("127.0.0.1", 29294);
			c.ServerName = "Test Server";
			c.Gamemode = "TDP";
			c.Map = "DustyDew";

			while (true)
			{
				c.Tick();
				Thread.Sleep(1);
			}
		}
	}

	private static async Task<bool> OnClientConnecting(IPAddress ip)
	{
		Console.WriteLine(ip + " is connecting.");
		return true;
	}

	private static async Task OnGameServerConnected(GameServer server)
	{
		Console.WriteLine("Server " + server.ServerName + " was connected.");
	}

	private static async Task OnGameServerDisconnected(GameServer server)
	{
		Console.WriteLine("Server " + server.ServerName + " was disconnected. (" + server.TerminationReason + ")");
	}
}