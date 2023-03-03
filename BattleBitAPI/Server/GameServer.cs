using System.Net;
using System.Net.Sockets;
using BattleBitAPI.Common.Enums;
using BattleBitAPI.Common.Extentions;
using BattleBitAPI.Networking;
using CommunityServerAPI.BattleBitAPI;

namespace BattleBitAPI.Server
{
    public class GameServer
    {
        // ---- Public Variables ---- 
        public TcpClient Socket { get; private set; }

        /// <summary>
        /// Is game server connected to our server?
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
        /// Reason why connection was terminated.
        /// </summary>
        public string TerminationReason { get; private set; }

        // ---- Private Variables ---- 
        private byte[] mKeepAliveBuffer;
        private Common.Serialization.Stream mWriteStream;
        private Common.Serialization.Stream mReadStream;
        private uint mReadPackageSize;
        private long mLastPackageReceived;
        private long mLastPackageSent;

        // ---- Constrction ---- 
        public GameServer(TcpClient socket, IPAddress iP, int port, bool isPasswordProtected, string serverName, string gamemode, string map, MapSize mapSize, MapDayNight dayNight, int currentPlayers, int inQueuePlayers, int maxPlayers, string loadingScreenText, string serverRulesText)
        {
            this.IsConnected = true;
            this.Socket = socket;

            this.GameIP = iP;
            this.GamePort = port;
            this.IsPasswordProtected = isPasswordProtected;
            this.ServerName = serverName;
            this.Gamemode = gamemode;
            this.Map = map;
            this.MapSize = mapSize;
            this.DayNight = dayNight;
            this.CurrentPlayers = currentPlayers;
            this.InQueuePlayers = inQueuePlayers;
            this.MaxPlayers = maxPlayers;
            this.LoadingScreenText = loadingScreenText;
            this.ServerRulesText = serverRulesText;

            this.TerminationReason = string.Empty;

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

        // ---- Tick ----
        public async Task Tick()
        {
            if (!this.IsConnected)
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
                    this.mLastPackageReceived = Extentions.TickCount;

                    //Do we know the package size?
                    if (this.mReadPackageSize == 0)
                    {
                        const int sizeToRead = 4;
                        int leftSizeToRead = sizeToRead - this.mReadStream.WritePosition;

                        int read = await networkStream.ReadAsync(this.mReadStream.Buffer, this.mReadStream.WritePosition, leftSizeToRead);
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

                        int read = await networkStream.ReadAsync(this.mReadStream.Buffer, this.mReadStream.WritePosition, leftSizeToRead);
                        if (read <= 0)
                            throw new Exception("Connection was terminated.");

                        this.mReadStream.WritePosition += read;

                        //Do we have the package?
                        if (this.mReadStream.WritePosition >= mReadPackageSize)
                        {
                            this.mReadPackageSize = 0;

                            await mExecutePackage(this.mReadStream);

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
                    throw new Exception("Game server timedout.");

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
        private async Task mExecutePackage(Common.Serialization.Stream stream)
        {
            var communcation = (NetworkCommuncation)stream.ReadInt8();
            switch (communcation)
            {

            }
        }

        private void mClose(string reason)
        {
            if (this.IsConnected)
            {
                this.TerminationReason = reason;
                this.IsConnected = false;

                this.mWriteStream = null;
                this.mReadStream = null;
            }
        }
    }
}
