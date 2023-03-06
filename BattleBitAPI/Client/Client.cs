#region

using System.Diagnostics;
using System.Net.Sockets;

using BattleBitAPI.Common.Enums;
using BattleBitAPI.Networking;

using CommunityServerAPI.BattleBitAPI;
using CommunityServerAPI.BattleBitAPI.Common.Extentions;

using Stream = BattleBitAPI.Common.Serialization.Stream;

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

	// ---- Main Tick ----
	public void Tick()
	{
		//Are we connecting?
		if (mIsConnectingFlag)
			return;

		//Have we connected?
		if (!IsConnected)
		{
			//Attempt to connect to server async.
			mIsConnectingFlag = true;

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
						using (var hail = Stream.Get())
						{
							hail.Write((byte)NetworkCommuncation.Hail);
							hail.Write((ushort)GamePort);
							hail.Write(IsPasswordProtected);
							hail.Write(ServerName);
							hail.Write(Gamemode);
							hail.Write(Map);
							hail.Write((byte)MapSize);
							hail.Write((byte)DayNight);
							hail.Write((byte)CurrentPlayers);
							hail.Write((byte)InQueuePlayers);
							hail.Write((byte)MaxPlayers);
							hail.Write(LoadingScreenText);
							hail.Write(ServerRulesText);

							//Send our hail package.
							networkStream.Write(hail.Buffer, 0, hail.WritePosition);
							networkStream.Flush();
						}

						//Sadly can not use Task Async here, Unity isn't great with tasks.
						var watch = Stopwatch.StartNew();

						//Read the first byte.
						var response = NetworkCommuncation.None;
						while (watch.ElapsedMilliseconds < Const.HailConnectTimeout)
						{
							if (mSocket.Available > 0)
							{
								var data = networkStream.ReadByte();
								if (data >= 0)
								{
									response = (NetworkCommuncation)data;
									break;
								}
							}
							Thread.Sleep(1);
						}

						//Were we accepted.
						if (response == NetworkCommuncation.Accepted)
						{
							//We are accepted.
							mIsConnectingFlag = false;
							IsConnected = true;

							mOnConnectedToServer();
						}
						else
						{
							//Did we at least got a response?
							if (response == NetworkCommuncation.None)
								throw new Exception("Server did not respond to your connect request.");

							//Try to read our deny reason.
							if (response == NetworkCommuncation.Denied && mSocket.Available > 0)
							{
								string errorString = null;

								using (var readStream = Stream.Get())
								{
									readStream.WritePosition = networkStream.Read(readStream.Buffer, 0, mSocket.Available);
									if (!readStream.TryReadString(out errorString))
										errorString = null;
								}

								if (errorString != null)
									throw new Exception(errorString);
							}

							throw new Exception("Server denied our connect request with an unknown reason.");
						}
					}
					catch (Exception e)
					{
						mIsConnectingFlag = false;

						mLogError("Unable to connect to API server: " + e.Message);
						return;
					}
				}, null);
			}
			catch
			{
				mIsConnectingFlag = false;
			}

			//We haven't connected yet.
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
					var leftSizeToRead = sizeToRead - mReadStream.WritePosition;

					var read = networkStream.Read(mReadStream.Buffer, mReadStream.WritePosition, leftSizeToRead);
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

					var read = networkStream.Read(mReadStream.Buffer, mReadStream.WritePosition, leftSizeToRead);
					if (read <= 0)
						throw new Exception("Connection was terminated.");

					mReadStream.WritePosition += read;

					//Do we have the package?
					if (mReadStream.WritePosition >= mReadPackageSize)
					{
						mReadPackageSize = 0;

						mExecutePackage(mReadStream);

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
		var communcation = (NetworkCommuncation)stream.ReadInt8();
		switch (communcation)
		{
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
