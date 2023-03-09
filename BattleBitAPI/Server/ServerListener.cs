#region

using System.Net;
using System.Net.Sockets;

using BattleBitAPI.Common.Enums;
using BattleBitAPI.Networking;

using CommunityServerAPI.BattleBitAPI;
using CommunityServerAPI.BattleBitAPI.Common.Extentions;
using CommunityServerAPI.BattleBitAPI.Packets;

#endregion

namespace BattleBitAPI.Server;

public class ServerListener : IDisposable
{
	// --- Private ---
	private TcpListener mSocket;

	// --- Construction ---
	public ServerListener()
	{
	}

	// --- Public ---
	public bool IsListening { get; private set; }

	public bool IsDisposed { get; private set; }

	public int ListeningPort { get; private set; }

	// --- Events ---
	/// <summary>
	///     Fired when an attempt made to connect to the server.
	///     Connection will be allowed if function returns true, otherwise will be blocked.
	///     Default, any connection attempt will be accepted.
	/// </summary>
	public Func<IPAddress, Task<bool>> OnGameServerConnecting { get; set; }

	/// <summary>
	///     Fired when a game server connects.
	/// </summary>
	public Func<GameServer, Task> OnGameServerConnected { get; set; }

	/// <summary>
	///     Fired when a game server disconnects. Check (server.TerminationReason) to see the reason.
	/// </summary>
	public Func<GameServer, Task> OnGameServerDisconnected { get; set; }

	// --- Disposing ---
	public void Dispose()
	{
		//Already disposed?
		if (IsDisposed)
			return;
		IsDisposed = true;

		if (IsListening)
			Stop();
	}

	// --- Starting ---
	public void Start(IPAddress bindIP, int port)
	{
		if (IsDisposed)
			throw new ObjectDisposedException(GetType().FullName);
		if (bindIP == null)
			throw new ArgumentNullException(nameof(bindIP));
		if (IsListening)
			throw new Exception("Server is already listening.");

		mSocket = new TcpListener(bindIP, port);
		mSocket.Start();

		ListeningPort = port;
		IsListening = true;

		mMainLoop();
	}

	public void Start(int port)
	{
		Start(IPAddress.Loopback, port);
	}

	// --- Stopping ---
	public void Stop()
	{
		if (IsDisposed)
			throw new ObjectDisposedException(GetType().FullName);
		if (!IsListening)
			throw new Exception("Already not running.");

		try
		{
			mSocket.Stop();
		}
		catch
		{
		}

		mSocket = null;
		ListeningPort = 0;
		IsListening = true;
	}

	// --- Main Loop ---
	private async Task mMainLoop()
	{
		while (IsListening)
		{
			var client = await mSocket.AcceptTcpClientAsync();
			mInternalOnClientConnecting(client);
		}
	}

	private async Task mInternalOnClientConnecting(TcpClient client)
	{
		var ip = (client.Client.RemoteEndPoint as IPEndPoint).Address;

		var allow = true;
		if (OnGameServerConnecting != null)
			allow = await OnGameServerConnecting(ip);

		if (!allow)
		{
			//Connection is not allowed from this IP.
			client.SafeClose();
			return;
		}

		GameServer server = null;
		try
		{
			using (var source = new CancellationTokenSource(Const.HailConnectTimeout))
			{
				using (var readStream = new MemoryStream())
				{
					var networkStream = client.GetStream();

					using (var reader = new BinaryReader(readStream))
					{
						readStream.Seek(0, SeekOrigin.Begin);
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the package type");
						var type = (NetworkCommuncation)reader.ReadByte();
						if (type != NetworkCommuncation.Hail)
							throw new Exception("Incoming package wasn't hail.");

						var packet = new HailPacket();
						if (!packet.TryRead(reader, CancellationToken.None))
							throw new Exception("Failed to deserialize packet");

						server = new GameServer(client,
							ip,
							packet.GamePort,
							packet.IsPasswordProtected,
							packet.ServerName,
							packet.Gamemode,
							packet.Map,
							packet.MapSize,
							packet.DayNight,
							packet.CurrentPlayers,
							packet.InQueuePlayers,
							packet.MaxPlayers,
							packet.LoadingScreenText,
							packet.ServerRulesText);

						//Send accepted notification.
						networkStream.WriteByte((byte)NetworkCommuncation.Accepted);
					}

				}
			}
		}
		catch (Exception e)
		{
			try
			{
				Console.WriteLine(e.Message);

				var networkStream = client.GetStream();
				using (var pck = new BinaryWriter(networkStream))
				{
					pck.Write((byte)NetworkCommuncation.Denied);
					pck.Write(e.Message);
				}
				await networkStream.FlushAsync();
			}
			catch
			{
			}


			client.SafeClose();
			return;
		}

		//Call the callback.
		if (OnGameServerConnected != null)
			await OnGameServerConnected.Invoke(server);

		//Set the buffer sizes.
		client.ReceiveBufferSize = Const.MaxNetworkPackageSize;
		client.SendBufferSize = Const.MaxNetworkPackageSize;

		//Join to main server loop.
		await mHandleGameServer(server);
	}

	private async Task mHandleGameServer(GameServer server)
	{
		while (server.IsConnected)
		{
			await server.Tick();
			await Task.Delay(1);
		}

		if (OnGameServerDisconnected != null)
			await OnGameServerDisconnected.Invoke(server);
	}
}
