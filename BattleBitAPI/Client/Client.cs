using BattleBitAPI.Common.Enums;
using BattleBitAPI.Common.Extentions;
using BattleBitAPI.Common.Serialization;
using BattleBitAPI.Networking;
using CommunityServerAPI.BattleBitAPI;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace BattleBitAPI.Client
{
    // This class was created mainly for Unity Engine, for this reason, Task async was not implemented.
    public class Client
    {
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

        // ---- Private Variables ---- 
        private TcpClient mSocket;
        private string mDestination;
        private int mPort;
        private byte[] mKeepAliveBuffer;
        private Common.Serialization.Stream mWriteStream;
        private Common.Serialization.Stream mReadStream;
        private uint mReadPackageSize;
        private long mLastPackageReceived;
        private long mLastPackageSent;
        private bool mIsConnectingFlag;

        // ---- Construction ---- 
        public Client(string destination, int port)
        {
            this.mDestination = destination;
            this.mPort = port;

            this.mWriteStream = new Common.Serialization.Stream()
            {
                Buffer = new byte[Const.MaxNetworkPackageSize],
                InPool = false,
                ReadPosition = 0,
                WritePosition = 0,
            };
            this.mReadStream = new Common.Serialization.Stream()
            {
                Buffer = new byte[Const.MaxNetworkPackageSize],
                InPool = false,
                ReadPosition = 0,
                WritePosition = 0,
            };
            this.mKeepAliveBuffer = new byte[4]
            {
                0,0,0,0,
            };

            this.mLastPackageReceived = Extentions.TickCount;
            this.mLastPackageSent = Extentions.TickCount;
        }

        // ---- Main Tick ---- 
        public void Tick()
        {
            //Are we connecting?
            if (mIsConnectingFlag)
                return;

            //Have we connected?
            if (!this.IsConnected)
            {
                //Attempt to connect to server async.
                this.mIsConnectingFlag = true;

                //Dispose old client if exist.
                if (this.mSocket != null)
                {
                    try { this.mSocket.Close(); } catch { }
                    try { this.mSocket.Dispose(); } catch { }
                    this.mSocket = null;
                }

                //Create new client
                this.mSocket = new TcpClient();
                this.mSocket.SendBufferSize = Const.MaxNetworkPackageSize;
                this.mSocket.ReceiveBufferSize = Const.MaxNetworkPackageSize;

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
                            using (var hail = BattleBitAPI.Common.Serialization.Stream.Get())
                            {
                                hail.Write((byte)NetworkCommuncation.Hail);
                                hail.Write((ushort)this.GamePort);
                                hail.Write(this.IsPasswordProtected);
                                hail.Write(this.ServerName);
                                hail.Write(this.Gamemode);
                                hail.Write(this.Map);
                                hail.Write((byte)this.MapSize);
                                hail.Write((byte)this.DayNight);
                                hail.Write((byte)this.CurrentPlayers);
                                hail.Write((byte)this.InQueuePlayers);
                                hail.Write((byte)this.MaxPlayers);
                                hail.Write(this.LoadingScreenText);
                                hail.Write(this.ServerRulesText);

                                //Send our hail package.
                                networkStream.Write(hail.Buffer, 0, hail.WritePosition);
                                networkStream.Flush();
                            }

                            //Sadly can not use Task Async here, Unity isn't great with tasks.
                            var watch = Stopwatch.StartNew();

                            //Read the first byte.
                            NetworkCommuncation response = NetworkCommuncation.None;
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
                                this.mIsConnectingFlag = false;
                                this.IsConnected = true;

                                this.mLastPackageReceived = Extentions.TickCount;
                                this.mLastPackageSent = Extentions.TickCount;

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

                                    using (var readStream = BattleBitAPI.Common.Serialization.Stream.Get())
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

                            this.mIsConnectingFlag = false;

                            mLogError("Unable to connect to API server: " + e.Message);
                            return;
                        }

                    }, null);
                }
                catch
                {
                    this.mIsConnectingFlag = false;
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
                    this.mLastPackageReceived = Extentions.TickCount;

                    //Do we know the package size?
                    if (this.mReadPackageSize == 0)
                    {
                        const int sizeToRead = 4;
                        int leftSizeToRead = sizeToRead - this.mReadStream.WritePosition;

                        int read = networkStream.Read(this.mReadStream.Buffer, this.mReadStream.WritePosition, leftSizeToRead);
                        if (read <= 0)
                            throw new Exception("Connection was terminated.");

                        this.mReadStream.WritePosition += read;

                        //Did we receive the package?
                        if (this.mReadStream.WritePosition >= 4)
                        {
                            //Read the package size
                            this.mReadPackageSize = this.mReadStream.ReadUInt32();

                            if (this.mReadPackageSize > Const.MaxNetworkPackageSize)
                                throw new Exception("Incoming package was larger than 'Conts.MaxNetworkPackageSize'");

                            //Is this keep alive package?
                            if (this.mReadPackageSize == 0)
                            {
                                Console.WriteLine("Keep alive was received.");
                            }

                            //Reset the stream.
                            this.mReadStream.Reset();
                        }
                    }
                    else
                    {
                        int sizeToRead = (int)mReadPackageSize;
                        int leftSizeToRead = sizeToRead - this.mReadStream.WritePosition;

                        int read = networkStream.Read(this.mReadStream.Buffer, this.mReadStream.WritePosition, leftSizeToRead);
                        if (read <= 0)
                            throw new Exception("Connection was terminated.");

                        this.mReadStream.WritePosition += read;

                        //Do we have the package?
                        if (this.mReadStream.WritePosition >= mReadPackageSize)
                        {
                            this.mReadPackageSize = 0;

                            mExecutePackage(this.mReadStream);

                            //Reset
                            this.mReadStream.Reset();
                        }
                    }
                }

                //Send the network packages.
                if (this.mWriteStream.WritePosition > 0)
                {
                    lock (this.mWriteStream)
                    {
                        if (this.mWriteStream.WritePosition > 0)
                        {
                            networkStream.Write(this.mWriteStream.Buffer, 0, this.mWriteStream.WritePosition);
                            this.mWriteStream.WritePosition = 0;

                            this.mLastPackageSent = Extentions.TickCount;
                        }
                    }
                }

                //Are we timed out?
                if ((Extentions.TickCount - this.mLastPackageReceived) > Const.NetworkTimeout)
                    throw new Exception("server timedout.");

                //Send keep alive if needed
                if ((Extentions.TickCount - this.mLastPackageSent) > Const.NetworkKeepAlive)
                {
                    //Send keep alive.
                    networkStream.Write(this.mKeepAliveBuffer, 0, 4);

                    this.mLastPackageSent = Extentions.TickCount;

                    Console.WriteLine("Keep alive was sent.");
                }
            }
            catch (Exception e)
            {
                mClose(e.Message);
            }
        }

        // ---- Internal ----
        private void mExecutePackage(Common.Serialization.Stream stream)
        {
            var communcation = (NetworkCommuncation)stream.ReadInt8();
            switch (communcation)
            {

            }
        }

        // ---- Callbacks ---- 
        private void mOnConnectedToServer()
        {
            Console.WriteLine("Connected to server.");
        }
        private void mOnDisconnectedFromServer(string reason)
        {
            Console.WriteLine("Disconnected from server (" + reason + ").");
        }

        // ---- Private ---- 
        private void mLogError(string str)
        {
            Console.WriteLine(str);
        }
        private void mClose(string reason)
        {
            if (this.IsConnected)
            {
                this.IsConnected = false;

                //Dispose old client if exist.
                if (this.mSocket != null)
                {
                    try { this.mSocket.Close(); } catch { }
                    try { this.mSocket.Dispose(); } catch { }
                    this.mSocket = null;
                }

                mOnDisconnectedFromServer(reason);
            }
        }
    }
}
