using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using BattleBitAPI.Common;
using BattleBitAPI.Common.Extentions;
using BattleBitAPI.Networking;
using CommunityServerAPI.BattleBitAPI;

namespace BattleBitAPI.Server
{
    public class GameServer : IDisposable
    {
        // ---- Public Variables ---- 
        public TcpClient Socket { get; private set; }

        public ulong ServerHash { get; private set; }
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
        public ServerSettings Settings { get; private set; }
        public MapRotation MapRotation { get; private set; }
        public GamemodeRotation GamemodeRotation { get; private set; }

        /// <summary>
        /// Reason why connection was terminated.
        /// </summary>
        public string TerminationReason { get; private set; }
        internal bool ReconnectFlag { get; set; }

        // ---- Private Variables ---- 
        private byte[] mKeepAliveBuffer;
        private Func<GameServer, mInternalResources, Common.Serialization.Stream, Task> mExecutionFunc;
        private Common.Serialization.Stream mWriteStream;
        private Common.Serialization.Stream mReadStream;
        private uint mReadPackageSize;
        private long mLastPackageReceived;
        private long mLastPackageSent;
        private bool mIsDisposed;
        private mInternalResources mInternal;
        private StringBuilder mBuilder;
        private bool mWantsToCloseConnection;

        // ---- Construction ---- 
        public GameServer(TcpClient socket, mInternalResources resources, Func<GameServer, mInternalResources, Common.Serialization.Stream, Task> func, IPAddress iP, int port, bool isPasswordProtected, string serverName, string gamemode, string map, MapSize mapSize, MapDayNight dayNight, int currentPlayers, int inQueuePlayers, int maxPlayers, string loadingScreenText, string serverRulesText)
        {
            this.IsConnected = true;
            this.Socket = socket;
            this.mInternal = resources;
            this.mExecutionFunc = func;
            this.mBuilder = new StringBuilder(1024);

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

            this.ServerHash = ((ulong)port << 32) | (ulong)iP.ToUInt();
            this.Settings = new ServerSettings(resources);
            this.MapRotation = new MapRotation(resources);
            this.GamemodeRotation = new GamemodeRotation(resources);
        }

        // ---- Tick ----
        public async Task Tick()
        {
            if (!this.IsConnected)
                return;
            if (this.mIsDisposed)
                return;

            if (this.mInternal.IsDirtySettings)
            {
                this.mInternal.IsDirtySettings = false;

                //Send new settings
                using (var pck = Common.Serialization.Stream.Get())
                {
                    pck.Write((byte)NetworkCommuncation.SetNewRoomSettings);
                    this.mInternal.Settings.Write(pck);
                    WriteToSocket(pck);
                }
            }
            if (this.mInternal.MapRotationDirty)
            {
                this.mInternal.MapRotationDirty = false;
                this.mBuilder.Clear();

                this.mBuilder.Append("setmaprotation ");
                lock (this.mInternal.MapRotation)
                    foreach (var map in this.mInternal.MapRotation)
                    {
                        this.mBuilder.Append(map);
                        this.mBuilder.Append(',');
                    }
                this.ExecuteCommand(this.mBuilder.ToString());
            }
            if (this.mInternal.GamemodeRotationDirty)
            {
                this.mInternal.GamemodeRotationDirty = false;
                this.mBuilder.Clear();

                this.mBuilder.Append("setgamemoderotation ");
                lock (this.mInternal.GamemodeRotation)
                    foreach (var gamemode in this.mInternal.GamemodeRotation)
                    {
                        this.mBuilder.Append(gamemode);
                        this.mBuilder.Append(',');
                    }
                this.ExecuteCommand(this.mBuilder.ToString());
            }

            try
            {
                //Are we still connected on socket level?
                if (!Socket.Connected)
                {
                    mClose("Connection was terminated.");
                    return;
                }

                //Did user requested to close connection?
                if (this.mWantsToCloseConnection)
                {
                    mClose(this.TerminationReason);
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

                            await this.mExecutionFunc(this, this.mInternal, this.mReadStream);

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
                            networkStream.WriteAsync(this.mWriteStream.Buffer, 0, this.mWriteStream.WritePosition);
                            this.mWriteStream.WritePosition = 0;

                            this.mLastPackageSent = Extentions.TickCount;
                        }
                    }
                }

                //Are we timed out?
                if ((Extentions.TickCount - this.mLastPackageReceived) > Const.NetworkTimeout)
                    throw new Exception("Timedout.");

                //Send keep alive if needed
                if ((Extentions.TickCount - this.mLastPackageSent) > Const.NetworkKeepAlive)
                {
                    //Send keep alive.
                    await networkStream.WriteAsync(this.mKeepAliveBuffer, 0, 4);
                    await networkStream.FlushAsync();
                    this.mLastPackageSent = Extentions.TickCount;
                }
            }
            catch (Exception e)
            {
                mClose(e.Message);
            }
        }

        // ---- Team ----
        public IEnumerable<Player> GetAllPlayers()
        {
            var list = new List<Player>(254);
            list.AddRange(this.mInternal.Players.Values);
            return list;
        }

        // ---- Functions ----
        public void WriteToSocket(Common.Serialization.Stream pck)
        {
            lock (mWriteStream)
            {
                mWriteStream.Write((uint)pck.WritePosition);
                mWriteStream.Write(pck.Buffer, 0, pck.WritePosition);
            }
        }
        public void ExecuteCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return;

            int bytesLong = System.Text.Encoding.UTF8.GetByteCount(cmd);
            lock (mWriteStream)
            {
                mWriteStream.Write((uint)(1 + 2 + bytesLong));
                mWriteStream.Write((byte)NetworkCommuncation.ExecuteCommand);
                mWriteStream.Write(cmd);
            }
        }

        public void SetNewPassword(string newPassword)
        {
            ExecuteCommand("setpass " + newPassword);
        }
        public void SetPingLimit(int newPing)
        {
            ExecuteCommand("setmaxping " + newPing);
        }
        public void AnnounceShort(string msg)
        {
            ExecuteCommand("an " + msg);
        }
        public void AnnounceLong(string msg)
        {
            ExecuteCommand("ann " + msg);
        }
        public void UILogOnServer(string msg, float messageLifetime)
        {
            ExecuteCommand("serverlog " + msg + " " + messageLifetime);
        }
        public void ForceStartGame()
        {
            ExecuteCommand("forcestart");
        }
        public void ForceEndGame()
        {
            ExecuteCommand("endgame");
        }
        public void SayToChat(string msg)
        {
            ExecuteCommand("say " + msg);
        }

        public void StopServer()
        {
            ExecuteCommand("stop");
        }
        public void CloseServer()
        {
            ExecuteCommand("notifyend");
        }
        public void KickAllPlayers()
        {
            ExecuteCommand("kick all");
        }
        public void Kick(ulong steamID, string reason)
        {
            ExecuteCommand("kick " + steamID + " " + reason);
        }
        public void Kick(Player player, string reason)
        {
            Kick(player.SteamID, reason);
        }
        public void Kill(ulong steamID)
        {
            ExecuteCommand("kill " + steamID);
        }
        public void Kill(Player player)
        {
            Kill(player.SteamID);
        }
        public void ChangeTeam(ulong steamID)
        {
            ExecuteCommand("changeteam " + steamID);
        }
        public void ChangeTeam(Player player)
        {
            ChangeTeam(player.SteamID);
        }
        public void KickFromSquad(ulong steamID)
        {
            ExecuteCommand("squadkick " + steamID);
        }
        public void KickFromSquad(Player player)
        {
            KickFromSquad(player.SteamID);
        }
        public void DisbandPlayerSSquad(ulong steamID)
        {
            ExecuteCommand("squaddisband " + steamID);
        }
        public void DisbandPlayerCurrentSquad(Player player)
        {
            DisbandPlayerSSquad(player.SteamID);
        }
        public void PromoteSquadLeader(ulong steamID)
        {
            ExecuteCommand("squadpromote " + steamID);
        }
        public void PromoteSquadLeader(Player player)
        {
            PromoteSquadLeader(player.SteamID);
        }
        public void WarnPlayer(ulong steamID, string msg)
        {
            ExecuteCommand("warn " + steamID + " " + msg);
        }
        public void WarnPlayer(Player player, string msg)
        {
            WarnPlayer(player.SteamID, msg);
        }
        public void MessageToPlayer(ulong steamID, string msg)
        {
            ExecuteCommand("msg " + steamID + " " + msg);
        }
        public void MessageToPlayer(Player player, string msg)
        {
            MessageToPlayer(player.SteamID, msg);
        }
        public void SetRoleTo(ulong steamID, GameRole role)
        {
            ExecuteCommand("setrole " + steamID + " " + role);
        }
        public void SetRoleTo(Player player, GameRole role)
        {
            SetRoleTo(player.SteamID, role);
        }
        public void SpawnPlayer(ulong steamID, PlayerLoadout loadout, PlayerWearings wearings, Vector3 position, Vector3 lookDirection, PlayerStand stand, float spawnProtection)
        {
            var request = new PlayerSpawnRequest()
            {
                Loadout = loadout,
                Wearings = wearings,
                RequestedPoint = PlayerSpawningPosition.Null,
                SpawnPosition = position,
                LookDirection = lookDirection,
                SpawnStand = stand,
                SpawnProtection = spawnProtection
            };

            //Respond back.
            using (var response = Common.Serialization.Stream.Get())
            {
                response.Write((byte)NetworkCommuncation.SpawnPlayer);
                response.Write(steamID);
                request.Write(response);
                response.Write((ushort)0);

                WriteToSocket(response);
            }
        }
        public void SpawnPlayer(Player player, PlayerLoadout loadout, PlayerWearings wearings, Vector3 position, Vector3 lookDirection, PlayerStand stand, float spawnProtection)
        {
            SpawnPlayer(player.SteamID, loadout, wearings, position, lookDirection, stand, spawnProtection);
        }

        // ---- Closing ----
        public void CloseConnection(string additionInfo = string.Empty)
        {
            if (string.IsNullOrWhiteSpace(additionInfo))
                this.TerminationReason = additionInfo;
            else
                this.TerminationReason = "User requested to terminate the connection";
            this.mWantsToCloseConnection = true;
        }
        private void mClose(string reason)
        {
            if (this.IsConnected)
            {
                this.TerminationReason = reason;
                this.IsConnected = false;
            }
        }

        // ---- Disposing ----
        public void Dispose()
        {
            if (this.mIsDisposed)
                return;
            this.mIsDisposed = true;

            if (this.mWriteStream != null)
            {
                this.mWriteStream.Dispose();
                this.mWriteStream = null;
            }

            if (this.mReadStream != null)
            {
                this.mReadStream.Dispose();
                this.mReadStream = null;
            }

            if (this.Socket != null)
            {
                this.Socket.SafeClose();
                this.Socket = null;
            }
        }

        // ---- Overrides ----
        public override string ToString()
        {
            return
                this.GameIP + ":" + this.GamePort + " - " +
                this.ServerName;
        }



        // ---- Internal ----
        public class mInternalResources
        {
            public Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>(254);

            public mRoomSettings Settings = new mRoomSettings();
            public bool IsDirtySettings;

            public HashSet<string> MapRotation = new HashSet<string>(8);
            public bool MapRotationDirty = false;

            public HashSet<string> GamemodeRotation = new HashSet<string>(8);
            public bool GamemodeRotationDirty = false;

            public void AddPlayer<TPlayer>(TPlayer player) where TPlayer : Player
            {
                lock (Players)
                {
                    Players.Remove(player.SteamID);
                    Players.Add(player.SteamID, player);
                }
            }
            public void RemovePlayer<TPlayer>(TPlayer player) where TPlayer : Player
            {
                lock (Players)
                    Players.Remove(player.SteamID);
            }
            public bool TryGetPlayer(ulong steamID, out Player result)
            {
                lock (Players)
                    return Players.TryGetValue(steamID, out result);
            }
        }
        public class mRoomSettings
        {
            public float DamageMultiplier = 1.0f;
            public bool BleedingEnabled = true;
            public bool StamineEnabled = false;
            public bool FriendlyFireEnabled = false;
            public bool HideMapVotes = true;
            public bool OnlyWinnerTeamCanVote = false;
            public bool HitMarkersEnabled = true;
            public bool PointLogEnabled = true;
            public bool SpectatorEnabled = true;

            public byte MedicLimitPerSquad = 8;
            public byte EngineerLimitPerSquad = 8;
            public byte SupportLimitPerSquad = 8;
            public byte ReconLimitPerSquad = 8;

            public void Write(Common.Serialization.Stream ser)
            {
                ser.Write(this.DamageMultiplier);
                ser.Write(this.BleedingEnabled);
                ser.Write(this.StamineEnabled);
                ser.Write(this.FriendlyFireEnabled);
                ser.Write(this.HideMapVotes);
                ser.Write(this.OnlyWinnerTeamCanVote);
                ser.Write(this.HitMarkersEnabled);
                ser.Write(this.PointLogEnabled);
                ser.Write(this.SpectatorEnabled);
                ser.Write(this.MedicLimitPerSquad);
                ser.Write(this.EngineerLimitPerSquad);
                ser.Write(this.SupportLimitPerSquad);
                ser.Write(this.ReconLimitPerSquad);
            }
            public void Read(Common.Serialization.Stream ser)
            {
                this.DamageMultiplier = ser.ReadFloat();
                this.BleedingEnabled = ser.ReadBool();
                this.StamineEnabled = ser.ReadBool();
                this.FriendlyFireEnabled = ser.ReadBool();
                this.HideMapVotes = ser.ReadBool();
                this.OnlyWinnerTeamCanVote = ser.ReadBool();
                this.HitMarkersEnabled = ser.ReadBool();
                this.PointLogEnabled = ser.ReadBool();
                this.SpectatorEnabled = ser.ReadBool();

                this.MedicLimitPerSquad = ser.ReadInt8();
                this.EngineerLimitPerSquad = ser.ReadInt8();
                this.SupportLimitPerSquad = ser.ReadInt8();
                this.ReconLimitPerSquad = ser.ReadInt8();
            }
        }
    }
}
