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

public class GameServer
{
	// ---- Private Variables ---- 
	private byte[] mKeepAliveBuffer;
	private long mLastPackageReceived;
	private long mLastPackageSent;
	private uint mReadPackageSize;
	private Stream mReadStream;
	private Stream mWriteStream;

	// ---- Constrction ---- 
	public GameServer(TcpClient socket, IPAddress iP, int port, bool isPasswordProtected, string serverName, string gamemode, string map, MapSize mapSize, MapDayNight dayNight, int currentPlayers, int inQueuePlayers, int maxPlayers, string loadingScreenText, string serverRulesText)
	{
		IsConnected = true;
		Socket = socket;

		GameIP = iP;
		GamePort = port;
		IsPasswordProtected = isPasswordProtected;
		ServerName = serverName;
		Gamemode = gamemode;
		Map = map;
		MapSize = mapSize;
		DayNight = dayNight;
		CurrentPlayers = currentPlayers;
		InQueuePlayers = inQueuePlayers;
		MaxPlayers = maxPlayers;
		LoadingScreenText = loadingScreenText;
		ServerRulesText = serverRulesText;

		TerminationReason = string.Empty;

		mWriteStream = new Stream()
		{
			Buffer = new byte[Const.MaxNetworkPackageSize],
			InPool = false,
			ReadPosition = 0,
			WritePosition = 0
		};
		mReadStream = new Stream()
		{
			Buffer = new byte[Const.MaxNetworkPackageSize],
			InPool = false,
			ReadPosition = 0,
			WritePosition = 0
		};
		mKeepAliveBuffer = new byte[4]
		{
			0, 0, 0, 0
		};

		mLastPackageReceived = Extensions.TickCount;
		mLastPackageSent = Extensions.TickCount;
	}

	// ---- Public Variables ---- 
	public TcpClient Socket { get; private set; }

    /// <summary>
    ///     Is game server connected to our server?
    /// </summary>
    public bool IsConnected { get; private set; }

	public IPAddress GameIP { get; private set; }

	public int GamePort { get; private set; }

	public bool IsPasswordProtected { get; private set; }

	public string ServerName { get; private set; }

	public string Gamemode { get; private set; }

	public string Map { get; private set; }

	public MapSize MapSize { get; private set; }

	public MapDayNight DayNight { get; private set; }

	public int CurrentPlayers { get; private set; }

	public int InQueuePlayers { get; private set; }

	public int MaxPlayers { get; private set; }

	public string LoadingScreenText { get; private set; }

	public string ServerRulesText { get; private set; }

    /// <summary>
    ///     Reason why connection was terminated.
    /// </summary>
    public string TerminationReason { get; private set; }

	// ---- Tick ----
	public async Task Tick()
	{
		if (!IsConnected)
			return;

		try
		{
			//Are we still connected on socket level?
			if (!Socket.Connected)
			{
				mClose("Connection was terminated.");
				return;
			}

			var networkStream = Socket.GetStream();

			//Read network packages.
			while (Socket.Available > 0)
			{
				mLastPackageReceived = Extensions.TickCount;

				//Do we know the package size?
				if (mReadPackageSize == 0)
				{
					const int sizeToRead = 4;
					var leftSizeToRead = sizeToRead - mReadStream.WritePosition;

					var read = await networkStream.ReadAsync(mReadStream.Buffer, mReadStream.WritePosition, leftSizeToRead);
					if (read <= 0)
						throw new Exception("Connection was terminated.");

					mReadStream.WritePosition += read;

					//Did we receive the package?
					if (mReadStream.WritePosition >= 4)
					{
						//Read the package size
						mReadPackageSize = mReadStream.ReadUInt32();

						if (mReadPackageSize > Const.MaxNetworkPackageSize)
							throw new Exception("Incoming package was larger than 'Conts.MaxNetworkPackageSize'");

						//Is this keep alive package?
						if (mReadPackageSize == 0)
							Console.WriteLine("Keep alive was received.");

						//Reset the stream.
						mReadStream.Reset();
					}
				}
				else
				{
					var sizeToRead = (int)mReadPackageSize;
					var leftSizeToRead = sizeToRead - mReadStream.WritePosition;

					var read = await networkStream.ReadAsync(mReadStream.Buffer, mReadStream.WritePosition, leftSizeToRead);
					if (read <= 0)
						throw new Exception("Connection was terminated.");

					mReadStream.WritePosition += read;

					//Do we have the package?
					if (mReadStream.WritePosition >= mReadPackageSize)
					{
						mReadPackageSize = 0;

						await mExecutePackage(mReadStream);

						//Reset
						mReadStream.Reset();
					}
				}
			}

			//Send the network packages.
			if (mWriteStream.WritePosition > 0)
				lock (mWriteStream)
				{
					if (mWriteStream.WritePosition > 0)
					{
						networkStream.Write(mWriteStream.Buffer, 0, mWriteStream.WritePosition);
						mWriteStream.WritePosition = 0;

						mLastPackageSent = Extensions.TickCount;
					}
				}

			//Are we timed out?
			if (Extensions.TickCount - mLastPackageReceived > Const.NetworkTimeout)
				throw new Exception("Game server timedout.");

			//Send keep alive if needed
			if (Extensions.TickCount - mLastPackageSent > Const.NetworkKeepAlive)
			{
				//Send keep alive.
				networkStream.Write(mKeepAliveBuffer, 0, 4);

				mLastPackageSent = Extensions.TickCount;

				Console.WriteLine("Keep alive was sent.");
			}
		}
		catch (Exception e)
		{
			mClose(e.Message);
		}
	}

	// ---- Internal ----
	private async Task mExecutePackage(Stream stream)
	{
		var communcation = (NetworkCommuncation)stream.ReadInt8();
		switch (communcation)
		{
		}
	}

	private void mClose(string reason)
	{
		if (IsConnected)
		{
			TerminationReason = reason;
			IsConnected = false;

			mWriteStream = null;
			mReadStream = null;
		}
	}
}