#region

using System.Net;
using System.Net.Sockets;

using BattleBitAPI.Common.Enums;
using BattleBitAPI.Networking;

using CommunityServerAPI.BattleBitAPI;
using CommunityServerAPI.BattleBitAPI.Common.Extentions;

using Stream = BattleBitAPI.Common.Serialization.Stream;

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
				using (var readStream = Stream.Get())
				{
					var networkStream = client.GetStream();

					//Read package type
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the package type");
						var type = (NetworkCommuncation)readStream.ReadInt8();
						if (type != NetworkCommuncation.Hail)
							throw new Exception("Incoming package wasn't hail.");
					}

					//Read port
					int gamePort;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 2, source.Token))
							throw new Exception("Unable to read the Port");
						gamePort = readStream.ReadUInt16();
					}

					//Read is Port protected
					bool isPasswordProtected;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the IsPasswordProtected");
						isPasswordProtected = readStream.ReadBool();
					}

					//Read the server name
					string serverName;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 2, source.Token))
							throw new Exception("Unable to read the ServerName Size");

						int stringSize = readStream.ReadUInt16();
						if (stringSize < Const.MinServerNameLength || stringSize > Const.MaxServerNameLength)
							throw new Exception("Invalid server name size");

						readStream.Reset();
						if (!await networkStream.TryRead(readStream, stringSize, source.Token))
							throw new Exception("Unable to read the ServerName");

						serverName = readStream.ReadString(stringSize);
					}

					//Read the gamemode
					string gameMode;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 2, source.Token))
							throw new Exception("Unable to read the gamemode Size");

						int stringSize = readStream.ReadUInt16();
						if (stringSize < Const.MinGamemodeNameLength || stringSize > Const.MaxGamemodeNameLength)
							throw new Exception("Invalid gamemode size");

						readStream.Reset();
						if (!await networkStream.TryRead(readStream, stringSize, source.Token))
							throw new Exception("Unable to read the gamemode");

						gameMode = readStream.ReadString(stringSize);
					}

					//Read the gamemap
					string gamemap;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 2, source.Token))
							throw new Exception("Unable to read the map size");

						int stringSize = readStream.ReadUInt16();
						if (stringSize < Const.MinMapNameLength || stringSize > Const.MaxMapNameLength)
							throw new Exception("Invalid map size");

						readStream.Reset();
						if (!await networkStream.TryRead(readStream, stringSize, source.Token))
							throw new Exception("Unable to read the map");

						gamemap = readStream.ReadString(stringSize);
					}

					//Read the mapSize
					MapSize size;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the MapSize");
						size = (MapSize)readStream.ReadInt8();
					}

					//Read the day night
					MapDayNight dayNight;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the MapDayNight");
						dayNight = (MapDayNight)readStream.ReadInt8();
					}

					//Current Players
					int currentPlayers;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the Current Players");
						currentPlayers = readStream.ReadInt8();
					}

					//Queue Players
					int queuePlayers;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the Queue Players");
						queuePlayers = readStream.ReadInt8();
					}

					//Max Players
					int maxPlayers;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 1, source.Token))
							throw new Exception("Unable to read the Max Players");
						maxPlayers = readStream.ReadInt8();
					}

					//Read Loading Screen Text
					string loadingScreenText;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 2, source.Token))
							throw new Exception("Unable to read the Loading Screen Text Size");

						int stringSize = readStream.ReadUInt16();
						if (stringSize < Const.MinLoadingScreenTextLength || stringSize > Const.MaxLoadingScreenTextLength)
							throw new Exception("Invalid server Loading Screen Text Size");

						if (stringSize > 0)
						{
							readStream.Reset();
							if (!await networkStream.TryRead(readStream, stringSize, source.Token))
								throw new Exception("Unable to read the Loading Screen Text");

							loadingScreenText = readStream.ReadString(stringSize);
						}
						else
						{
							loadingScreenText = string.Empty;
						}
					}

					//Read Server Rules Text
					string serverRulesText;
					{
						readStream.Reset();
						if (!await networkStream.TryRead(readStream, 2, source.Token))
							throw new Exception("Unable to read the Server Rules Text Size");

						int stringSize = readStream.ReadUInt16();
						if (stringSize < Const.MinServerRulesTextLength || stringSize > Const.MaxServerRulesTextLength)
							throw new Exception("Invalid server Server Rules Text Size");

						if (stringSize > 0)
						{
							readStream.Reset();
							if (!await networkStream.TryRead(readStream, stringSize, source.Token))
								throw new Exception("Unable to read the Server Rules Text");

							serverRulesText = readStream.ReadString(stringSize);
						}
						else
						{
							serverRulesText = string.Empty;
						}
					}

					server = new GameServer(client, ip, gamePort, isPasswordProtected, serverName, gameMode, gamemap, size, dayNight, currentPlayers, queuePlayers, maxPlayers, loadingScreenText, serverRulesText);

					//Send accepted notification.
					networkStream.WriteByte((byte)NetworkCommuncation.Accepted);
				}
			}
		}
		catch (Exception e)
		{
			try
			{
				Console.WriteLine(e.Message);

				var networkStream = client.GetStream();
				using (var pck = Stream.Get())
				{
					pck.Write((byte)NetworkCommuncation.Denied);
					pck.Write(e.Message);

					//Send denied notification.
					networkStream.Write(pck.Buffer, 0, pck.WritePosition);
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