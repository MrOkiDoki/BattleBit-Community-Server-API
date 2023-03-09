#region

using System.Net.Sockets;
using System.Text;

using BattleBitAPI.Common.Enums;
using BattleBitAPI.Networking;

using CommunityServerAPI.BattleBitAPI;
using CommunityServerAPI.BattleBitAPI.Common.Extentions;
using CommunityServerAPI.BattleBitAPI.Packets;


#endregion

namespace BattleBitAPI.Client;

// This class was created mainly for Unity Engine, for this reason, Task async was not implemented.
public class Client
{
	private string mDestination;
	private bool mIsConnectingFlag;
	private byte[] mKeepAliveBuffer;
	private long mLastPackageReceived;
	private long mLastPackageSent;
	private int mPort;
	private uint mReadPackageSize;
	private Stream mReadStream;

	// ---- Private Variables ----
	private TcpClient mSocket;
	private Stream mWriteStream;

	// ---- Construction ----
	public Client(string destination, int port)
	{
		mDestination = destination;
		mPort = port;

		mWriteStream = new MemoryStream();
		mReadStream = new MemoryStream();
		mKeepAliveBuffer = new byte[4]
		{
			0, 0, 0, 0
		};

		mLastPackageReceived = Extensions.TickCount;
		mLastPackageSent = Extensions.TickCount;
	}

	// ---- Public Variables ----
	public bool IsConnected { get; private set; }

	public int GamePort { get; set; } = 30000;

	public bool IsPasswordProtected { get; set; } = false;

	public string ServerName { get; set; } = "";

	public string Gamemode { get; set; } = "";

	public string Map { get; set; } = "";

	public MapSize MapSize { get; set; } = MapSize._16vs16;

	public MapDayNight DayNight { get; set; } = MapDayNight.Day;

	public int CurrentPlayers { get; set; } = 0;

	public int InQueuePlayers { get; set; } = 0;

	public int MaxPlayers { get; set; } = 16;

	public string LoadingScreenText { get; set; } = "";

	public string ServerRulesText { get; set; } = "";

	protected HailPacket CreateHail()
	{
		var packet = new HailPacket()
		{
			CurrentPlayers = CurrentPlayers,
			GamePort = GamePort,
			IsPasswordProtected = IsPasswordProtected,
			ServerName = ServerName,
			Gamemode = Gamemode,
			Map = Map,
			MapSize = MapSize,
			DayNight = DayNight,
			InQueuePlayers = InQueuePlayers,
			MaxPlayers = MaxPlayers,
			LoadingScreenText = LoadingScreenText,
			ServerRulesText = ServerRulesText
		};

		return packet;
	}

	/// <summary>
	/// Attempt to connect the client.
	/// Blocks the thread until connection succeeds or fails.
	/// </summary>
	/// <exception cref="Exception"></exception>
	protected bool HandleConnectionTick()
	{
		//Attempt to connect to server async.
		mIsConnectingFlag = true;

		//Dispose old client if exist.
		if (mSocket != null)
		{
			mSocket.SafeClose();
			mSocket = null;
		}

		//Create new client
		mSocket = new TcpClient();
		mSocket.SendBufferSize = Const.MaxNetworkPackageSize;
		mSocket.ReceiveBufferSize = Const.MaxNetworkPackageSize;

		//Attempt to connect.
		try
		{
			var state = mSocket.BeginConnect(mDestination, mPort, (x) =>
			{
				try
				{
					//Did we connect?
					mSocket.EndConnect(x);

					var networkStream = mSocket.GetStream();

					//Prepare our hail package and send it.
					if (!networkStream.TryWritePacket(CreateHail()))
						throw new Exception("Failed to send hail packet");

					//Read the first byte.
					var response = mSocket.AwaitResponse();

					switch (response)
					{
						case NetworkCommuncation.Accepted:
							mIsConnectingFlag = false;
							IsConnected = true;

							//	TODO: Do we really want to invoke OnConnectedToServer in this callback?
							mOnConnectedToServer();
							break;
						case NetworkCommuncation.None:
							throw new Exception("Server did not respond to your connect request.");
						case NetworkCommuncation.Denied:
							if (mSocket.Available <= 0)
								throw new Exception("Server denied our connect request with an unknown reason.");

							string errorString = null;

							using (var readStream = new MemoryStream())
							{
								networkStream.CopyTo(readStream, mSocket.Available);
								errorString = Encoding.UTF8.GetString(readStream.ToArray());

								if (errorString == string.Empty)
									errorString = null;
							}

							if (errorString != null)
								throw new Exception(errorString);
							throw new Exception("Server denied our connect request with an unknown reason.");
					}
				}
				catch (Exception e)
				{
					mIsConnectingFlag = false;

					mLogError("Unable to connect to API server: " + e.Message);
				}
			}, null);	//	mSocket.BeginConnect
		}
		catch
		{
			mIsConnectingFlag = false;
		}

		return IsConnected;
	}


	// ---- Main Tick ----
	public void Tick()
	{
		//Are we connecting?
		if (mIsConnectingFlag)
			return;

		//Have we connected?
		if (!IsConnected)
		{
			if (!HandleConnectionTick())
				//	Connection failed
				return;
		}

		//We are connected at this point.

		try
		{
			//Are we still connected on socket level?
			if (!mSocket.Connected)
			{
				mClose("Connection was terminated.");
				return;
			}

			var networkStream = mSocket.GetStream();

			//Read network packages.
			while (mSocket.Available > 0)
			{
				mLastPackageReceived = Extensions.TickCount;

				//Do we know the package size?
				if (mReadPackageSize == 0)
				{
					const int sizeToRead = 4;
					var leftSizeToRead = sizeToRead - mReadStream.Position;

					networkStream.CopyTo(mReadStream, (int) leftSizeToRead);
					//if (read <= 0)
					//	throw new Exception("Connection was terminated.");

					//mReadStream.Position += read;

					//Did we receive the package?
					if (mReadStream.Position >= 4)
					{
						using (var binaryreader = new BinaryReader(mReadStream))
						{
							//Read the package size
							mReadPackageSize = binaryreader.ReadUInt32();

							if (mReadPackageSize > Const.MaxNetworkPackageSize)
								throw new Exception("Incoming package was larger than 'Conts.MaxNetworkPackageSize'");

							//Is this keep alive package?
							if (mReadPackageSize == 0)
								Console.WriteLine("Keep alive was received.");
						}
						//Reset the stream.
						mReadStream.Seek(0, SeekOrigin.Begin);
					}
				}
				else
				{
					var sizeToRead = (int)mReadPackageSize;
					var leftSizeToRead = sizeToRead - mReadStream.Position;

					networkStream.CopyTo(mReadStream, (int)leftSizeToRead);
					//if (read <= 0)
					//	throw new Exception("Connection was terminated.");

					//Do we have the package?
					if (mReadStream.Position >= mReadPackageSize)
					{
						mReadPackageSize = 0;

						mExecutePackage(mReadStream);

						//Reset
						mReadStream.Seek(0, SeekOrigin.Begin);
					}
				}
			}

			//Send the network packages.
			if (mWriteStream.Position > 0)
				lock (mWriteStream)
				{
					if (mWriteStream.Position > 0)
					{
						mWriteStream.CopyTo(networkStream, (int) mWriteStream.Position);
						mWriteStream.Position = 0;

						mLastPackageSent = Extensions.TickCount;
					}
				}

			//Are we timed out?
			if (Extensions.TickCount - mLastPackageReceived > Const.NetworkTimeout)
				throw new Exception("server timedout.");

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
	private void mExecutePackage(Stream stream)
	{
		using (var reader = new BinaryReader(stream))
		{
			var communcation = (NetworkCommuncation)reader.ReadByte();
			switch (communcation)
			{
			}
		}
	}

	// ---- Callbacks ----
	private void mOnConnectedToServer()
	{
	}

	private void mOnDisconnectedFromServer(string reason)
	{
	}


	// ---- Private ----
	private void mLogError(string str)
	{
	}

	private void mClose(string reason)
	{
		if (IsConnected)
		{
			IsConnected = false;

			//Dispose old client if exist.
			if (mSocket != null)
			{
				try
				{
					mSocket.Close();
				}
				catch
				{
				}
				try
				{
					mSocket.Dispose();
				}
				catch
				{
				}
				mSocket = null;
			}

			mOnDisconnectedFromServer(reason);
		}
	}
}
