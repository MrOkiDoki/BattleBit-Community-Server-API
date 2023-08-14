using System.Diagnostics;
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
    public class GameServer<TPlayer> : System.IDisposable where TPlayer : Player<TPlayer>
    {
        // ---- Public Variables ---- 
        public ulong ServerHash => mInternal.ServerHash;
        public bool IsConnected => mInternal.IsConnected;
        public IPAddress GameIP => mInternal.GameIP;
        public int GamePort => mInternal.GamePort;

        public TcpClient Socket => mInternal.Socket;
        public bool IsPasswordProtected => mInternal.IsPasswordProtected;
        public string ServerName => mInternal.ServerName;
        public string Gamemode => mInternal.Gamemode;
        public string Map => mInternal.Map;
        public MapSize MapSize => mInternal.MapSize;
        public MapDayNight DayNight => mInternal.DayNight;
        public int CurrentPlayerCount => mInternal.CurrentPlayerCount;
        public int InQueuePlayerCount => mInternal.InQueuePlayerCount;
        public int MaxPlayerCount => mInternal.MaxPlayerCount;
        public string LoadingScreenText => mInternal.LoadingScreenText;
        public string ServerRulesText => mInternal.ServerRulesText;
        public ServerSettings<TPlayer> ServerSettings => mInternal.ServerSettings;
        public MapRotation<TPlayer> MapRotation => mInternal.MapRotation;
        public GamemodeRotation<TPlayer> GamemodeRotation => mInternal.GamemodeRotation;
        public RoundSettings<TPlayer> RoundSettings => mInternal.RoundSettings;
        public string TerminationReason => mInternal.TerminationReason;
        public bool ReconnectFlag => mInternal.ReconnectFlag;

        // ---- Private Variables ---- 
        private Internal mInternal;

        // ---- Tick ----
        public async Task Tick()
        {
            if (!this.IsConnected)
                return;

            if (this.mInternal.IsDirtyRoomSettings)
            {
                this.mInternal.IsDirtyRoomSettings = false;

                //Send new settings
                using (var pck = Common.Serialization.Stream.Get())
                {
                    pck.Write((byte)NetworkCommuncation.SetNewRoomSettings);
                    this.mInternal._RoomSettings.Write(pck);
                    WriteToSocket(pck);
                }
            }
            if (this.mInternal.IsDirtyMapRotation)
            {
                this.mInternal.IsDirtyMapRotation = false;
                this.mInternal.mBuilder.Clear();

                this.mInternal.mBuilder.Append("setmaprotation ");
                lock (this.mInternal._MapRotation)
                    foreach (var map in this.mInternal._MapRotation)
                    {
                        this.mInternal.mBuilder.Append(map);
                        this.mInternal.mBuilder.Append(',');
                    }
                this.ExecuteCommand(this.mInternal.mBuilder.ToString());
            }
            if (this.mInternal.IsDirtyGamemodeRotation)
            {
                this.mInternal.IsDirtyGamemodeRotation = false;
                this.mInternal.mBuilder.Clear();

                this.mInternal.mBuilder.Append("setgamemoderotation ");
                lock (this.mInternal._GamemodeRotation)
                {
                    foreach (var gamemode in this.mInternal._GamemodeRotation)
                    {
                        this.mInternal.mBuilder.Append(gamemode);
                        this.mInternal.mBuilder.Append(',');
                    }
                }
                this.ExecuteCommand(this.mInternal.mBuilder.ToString());
            }
            if (this.mInternal.IsDirtyRoundSettings)
            {
                this.mInternal.IsDirtyRoundSettings = false;

                //Send new round settings
                using (var pck = Common.Serialization.Stream.Get())
                {
                    pck.Write((byte)NetworkCommuncation.SetNewRoundState);
                    this.mInternal._RoundSettings.Write(pck);
                    WriteToSocket(pck);
                }
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
                if (this.mInternal.mWantsToCloseConnection)
                {
                    mClose(this.TerminationReason);
                    return;
                }

                var networkStream = Socket.GetStream();

                //Read network packages.
                while (Socket.Available > 0)
                {
                    this.mInternal.mLastPackageReceived = Extentions.TickCount;

                    //Do we know the package size?
                    if (this.mInternal.mReadPackageSize == 0)
                    {
                        const int sizeToRead = 4;
                        int leftSizeToRead = sizeToRead - this.mInternal.mReadStream.WritePosition;

                        int read = await networkStream.ReadAsync(this.mInternal.mReadStream.Buffer, this.mInternal.mReadStream.WritePosition, leftSizeToRead);
                        if (read <= 0)
                            throw new Exception("Connection was terminated.");

                        this.mInternal.mReadStream.WritePosition += read;

                        //Did we receive the package?
                        if (this.mInternal.mReadStream.WritePosition >= 4)
                        {
                            //Read the package size
                            this.mInternal.mReadPackageSize = this.mInternal.mReadStream.ReadUInt32();

                            if (this.mInternal.mReadPackageSize > Const.MaxNetworkPackageSize)
                                throw new Exception("Incoming package was larger than 'Conts.MaxNetworkPackageSize'");

                            this.mInternal.mReadStream.Reset();
                        }
                    }
                    else
                    {
                        int sizeToRead = (int)this.mInternal.mReadPackageSize;
                        int leftSizeToRead = sizeToRead - this.mInternal.mReadStream.WritePosition;

                        int read = await networkStream.ReadAsync(this.mInternal.mReadStream.Buffer, this.mInternal.mReadStream.WritePosition, leftSizeToRead);
                        if (read <= 0)
                            throw new Exception("Connection was terminated.");

                        this.mInternal.mReadStream.WritePosition += read;

                        //Do we have the package?
                        if (this.mInternal.mReadStream.WritePosition >= this.mInternal.mReadPackageSize)
                        {
                            this.mInternal.mReadPackageSize = 0;

                            await this.mInternal.mExecutionFunc(this, this.mInternal, this.mInternal.mReadStream);

                            //Reset
                            this.mInternal.mReadStream.Reset();
                        }
                    }
                }

                //Send the network packages.
                if (this.mInternal.mWriteStream.WritePosition > 0)
                {
                    lock (this.mInternal.mWriteStream)
                    {
                        if (this.mInternal.mWriteStream.WritePosition > 0)
                        {
                            networkStream.WriteAsync(this.mInternal.mWriteStream.Buffer, 0, this.mInternal.mWriteStream.WritePosition);
                            this.mInternal.mWriteStream.WritePosition = 0;

                            this.mInternal.mLastPackageSent = Extentions.TickCount;
                        }
                    }
                }

                //Are we timed out?
                if ((Extentions.TickCount - this.mInternal.mLastPackageReceived) > Const.NetworkTimeout)
                    throw new Exception("Timedout.");

                //Send keep alive if needed
                if ((Extentions.TickCount - this.mInternal.mLastPackageSent) > Const.NetworkKeepAlive)
                {
                    //Send keep alive.
                    await networkStream.WriteAsync(this.mInternal.mKeepAliveBuffer, 0, 4);
                    await networkStream.FlushAsync();
                    this.mInternal.mLastPackageSent = Extentions.TickCount;
                }
            }
            catch (Exception e)
            {
                mClose(e.Message);
            }
        }

        // ---- Team ----
        public IEnumerable<TPlayer> AllPlayers
        {
            get
            {
                var list = new List<TPlayer>(this.mInternal.Players.Values.Count);
                lock (this.mInternal.Players)
                {
                    foreach (var item in this.mInternal.Players.Values)
                        list.Add((TPlayer)item);
                }
                return list;
            }
        }
        public bool TryGetPlayer(ulong steamID, out TPlayer player)
        {
            lock (this.mInternal.Players)
            {
                if (this.mInternal.Players.TryGetValue(steamID, out var _player))
                {
                    player = (TPlayer)_player;
                    return true;
                }
            }

            player = default;
            return false;
        }

        // ---- Virtual ---- 
        public virtual async Task OnConnected()
        {

        }
        public virtual async Task OnTick()
        {

        }
        public virtual async Task OnReconnected()
        {

        }
        public virtual async Task OnDisconnected()
        {

        }
        public virtual async Task OnPlayerConnected(TPlayer player)
        {

        }
        public virtual async Task OnPlayerDisconnected(TPlayer player)
        {

        }
        public virtual async Task<bool> OnPlayerTypedMessage(TPlayer player, ChatChannel channel, string msg)
        {
            return true;
        }
        public virtual async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
        {
        }
        public virtual async Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
        {

        }
        public virtual async Task<bool> OnPlayerRequestingToChangeRole(TPlayer player, GameRole requestedRole)
        {
            return true;
        }
        public virtual async Task<bool> OnPlayerRequestingToChangeTeam(TPlayer player, Team requestedTeam)
        {
            return true;
        }
        public virtual async Task OnPlayerChangedRole(TPlayer player, GameRole role)
        {

        }
        public virtual async Task OnPlayerJoinedSquad(TPlayer player, Squads squad)
        {

        }
        public virtual async Task OnPlayerLeftSquad(TPlayer player, Squads squad)
        {

        }
        public virtual async Task OnPlayerChangeTeam(TPlayer player, Team team)
        {

        }
        public virtual async Task<OnPlayerSpawnArguments> OnPlayerSpawning(TPlayer player, OnPlayerSpawnArguments request)
        {
            return request;
        }
        public virtual async Task OnPlayerSpawned(TPlayer player)
        {

        }
        public virtual async Task OnPlayerDied(TPlayer player)
        {

        }
        public virtual async Task OnPlayerGivenUp(TPlayer player)
        {

        }
        public virtual async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<TPlayer> args)
        {

        }
        public virtual async Task OnAPlayerRevivedAnotherPlayer(TPlayer from,TPlayer to)
        {

        }
        public virtual async Task OnPlayerReported(TPlayer from, TPlayer to, ReportReason reason, string additional)
        {

        }
        public virtual async Task OnGameStateChanged(GameState oldState, GameState newState)
        {

        }
        public virtual async Task OnRoundStarted()
        {

        }
        public virtual async Task OnRoundEnded()
        {

        }

        // ---- Functions ----
        public void WriteToSocket(Common.Serialization.Stream pck)
        {
            lock (this.mInternal.mWriteStream)
            {
                this.mInternal.mWriteStream.Write((uint)pck.WritePosition);
                this.mInternal.mWriteStream.Write(pck.Buffer, 0, pck.WritePosition);
            }
        }
        public void ExecuteCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return;

            int bytesLong = System.Text.Encoding.UTF8.GetByteCount(cmd);
            lock (this.mInternal.mWriteStream)
            {
                this.mInternal.mWriteStream.Write((uint)(1 + 2 + bytesLong));
                this.mInternal.mWriteStream.Write((byte)NetworkCommuncation.ExecuteCommand);
                this.mInternal.mWriteStream.Write(cmd);
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
        public void Kick(Player<TPlayer> player, string reason)
        {
            Kick(player.SteamID, reason);
        }
        public void Kill(ulong steamID)
        {
            ExecuteCommand("kill " + steamID);
        }
        public void Kill(Player<TPlayer> player)
        {
            Kill(player.SteamID);
        }
        public void ChangeTeam(ulong steamID)
        {
            ExecuteCommand("changeteam " + steamID);
        }
        public void ChangeTeam(Player<TPlayer> player)
        {
            ChangeTeam(player.SteamID);
        }
        public void ChangeTeam(ulong steamID, Team team)
        {
            if (team == Team.TeamA)
                ExecuteCommand("changeteam " + steamID + " a");
            else if (team == Team.TeamB)
                ExecuteCommand("changeteam " + steamID + " b");
        }
        public void ChangeTeam(Player<TPlayer> player, Team team)
        {
            ChangeTeam(player.SteamID, team);
        }
        public void KickFromSquad(ulong steamID)
        {
            ExecuteCommand("squadkick " + steamID);
        }
        public void KickFromSquad(Player<TPlayer> player)
        {
            KickFromSquad(player.SteamID);
        }
        public void JoinSquad(ulong steamID, Squads targetSquad)
        {
            ExecuteCommand("setsquad " + steamID + " " + ((int)targetSquad));
        }
        public void JoinSquad(Player<TPlayer> player, Squads targetSquad)
        {
            JoinSquad(player.SteamID, targetSquad);
        }
        public void DisbandPlayerSquad(ulong steamID)
        {
            ExecuteCommand("squaddisband " + steamID);
        }
        public void DisbandPlayerCurrentSquad(Player<TPlayer> player)
        {
            DisbandPlayerSquad(player.SteamID);
        }
        public void PromoteSquadLeader(ulong steamID)
        {
            ExecuteCommand("squadpromote " + steamID);
        }
        public void PromoteSquadLeader(Player<TPlayer> player)
        {
            PromoteSquadLeader(player.SteamID);
        }
        public void WarnPlayer(ulong steamID, string msg)
        {
            ExecuteCommand("warn " + steamID + " " + msg);
        }
        public void WarnPlayer(Player<TPlayer> player, string msg)
        {
            WarnPlayer(player.SteamID, msg);
        }
        public void MessageToPlayer(ulong steamID, string msg)
        {
            ExecuteCommand("msg " + steamID + " " + msg);
        }
        public void MessageToPlayer(Player<TPlayer> player, string msg)
        {
            MessageToPlayer(player.SteamID, msg);
        }
        public void MessageToPlayer(ulong steamID, string msg, float fadeOutTime)
        {
            ExecuteCommand("msgf " + steamID + " " + fadeOutTime + " " + msg);
        }
        public void MessageToPlayer(Player<TPlayer> player, string msg, float fadeOutTime)
        {
            MessageToPlayer(player.SteamID, msg, fadeOutTime);
        }
        public void SetRoleTo(ulong steamID, GameRole role)
        {
            ExecuteCommand("setrole " + steamID + " " + role);
        }
        public void SetRoleTo(Player<TPlayer> player, GameRole role)
        {
            SetRoleTo(player.SteamID, role);
        }
        public void SpawnPlayer(ulong steamID, PlayerLoadout loadout, PlayerWearings wearings, Vector3 position, Vector3 lookDirection, PlayerStand stand, float spawnProtection)
        {
            var request = new OnPlayerSpawnArguments()
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
        public void SpawnPlayer(Player<TPlayer> player, PlayerLoadout loadout, PlayerWearings wearings, Vector3 position, Vector3 lookDirection, PlayerStand stand, float spawnProtection)
        {
            SpawnPlayer(player.SteamID, loadout, wearings, position, lookDirection, stand, spawnProtection);
        }
        public void SetHP(ulong steamID, float newHP)
        {
            ExecuteCommand("sethp " + steamID + " " + newHP);
        }
        public void SetHP(Player<TPlayer> player, float newHP)
        {
            SetHP(player.SteamID, newHP);
        }
        public void GiveDamage(ulong steamID, float damage)
        {
            ExecuteCommand("givedamage " + steamID + " " + damage);
        }
        public void GiveDamage(Player<TPlayer> player, float damage)
        {
            GiveDamage(player.SteamID, damage);
        }
        public void Heal(ulong steamID, float heal)
        {
            ExecuteCommand("heal " + steamID + " " + heal);
        }
        public void Heal(Player<TPlayer> player, float heal)
        {
            Heal(player.SteamID, heal);
        }
        public void SetRunningSpeedMultiplier(ulong steamID, float value)
        {
            ExecuteCommand("setrunningspeed " + steamID + " " + value);
        }
        public void SetRunningSpeedMultiplier(Player<TPlayer> player, float value)
        {
            SetRunningSpeedMultiplier(player.SteamID, value);
        }
        public void SetReceiveDamageMultiplier(ulong steamID, float value)
        {
            ExecuteCommand("setreceivedamagemultiplier " + steamID + " " + value);
        }
        public void SetReceiveDamageMultiplier(Player<TPlayer> player, float value)
        {
            SetReceiveDamageMultiplier(player.SteamID, value);
        }
        public void SetGiveDamageMultiplier(ulong steamID, float value)
        {
            ExecuteCommand("setgivedamagemultiplier " + steamID + " " + value);
        }
        public void SetGiveDamageMultiplier(Player<TPlayer> player, float value)
        {
            SetGiveDamageMultiplier(player.SteamID, value);
        }
        public void SetJumpMultiplier(ulong steamID, float value)
        {
            ExecuteCommand("setjumpmultiplier " + steamID + " " + value);
        }
        public void SetJumpMultiplier(Player<TPlayer> player, float value)
        {
            SetJumpMultiplier(player.SteamID, value);
        }
        public void SetFallDamageMultiplier(ulong steamID, float value)
        {
            ExecuteCommand("setfalldamagemultiplier " + steamID + " " + value);
        }
        public void SetFallDamageMultiplier(Player<TPlayer> player, float value)
        {
            SetFallDamageMultiplier(player.SteamID, value);
        }

        public void SetPrimaryWeapon(ulong steamID, WeaponItem item, int extraMagazines, bool clear = false)
        {
            using (var packet = Common.Serialization.Stream.Get())
            {
                packet.Write((byte)NetworkCommuncation.SetPlayerWeapon);
                packet.Write(steamID);
                packet.Write((byte)0);//Primary
                item.Write(packet);
                packet.Write((byte)extraMagazines);
                packet.Write(clear);

                WriteToSocket(packet);
            }
        }
        public void SetPrimaryWeapon(Player<TPlayer> player, WeaponItem item, int extraMagazines, bool clear = false)
        {
            SetPrimaryWeapon(player.SteamID, item, extraMagazines, clear);
        }
        public void SetSecondaryWeapon(ulong steamID, WeaponItem item, int extraMagazines, bool clear = false)
        {
            using (var packet = Common.Serialization.Stream.Get())
            {
                packet.Write((byte)NetworkCommuncation.SetPlayerWeapon);
                packet.Write(steamID);
                packet.Write((byte)1);//Secondary
                item.Write(packet);
                packet.Write((byte)extraMagazines);
                packet.Write(clear);

                WriteToSocket(packet);
            }
        }
        public void SetSecondaryWeapon(Player<TPlayer> player, WeaponItem item, int extraMagazines, bool clear = false)
        {
            SetSecondaryWeapon(player.SteamID, item, extraMagazines, clear);
        }
        public void SetFirstAid(ulong steamID, string tool, int extra, bool clear = false)
        {
            using (var packet = Common.Serialization.Stream.Get())
            {
                packet.Write((byte)NetworkCommuncation.SetPlayerGadget);
                packet.Write(steamID);
                packet.Write((byte)2);//first aid
                packet.Write(tool);
                packet.Write((byte)extra);
                packet.Write(clear);

                WriteToSocket(packet);
            }
        }
        public void SetFirstAid(Player<TPlayer> player, string tool, int extra, bool clear = false)
        {
            SetFirstAid(player.SteamID, tool, extra, clear);
        }
        public void SetLightGadget(ulong steamID, string tool, int extra, bool clear = false)
        {
            using (var packet = Common.Serialization.Stream.Get())
            {
                packet.Write((byte)NetworkCommuncation.SetPlayerGadget);
                packet.Write(steamID);
                packet.Write((byte)3);//Tool A
                packet.Write(tool);
                packet.Write((byte)extra);
                packet.Write(clear);

                WriteToSocket(packet);
            }
        }
        public void SetLightGadget(Player<TPlayer> player, string tool, int extra, bool clear = false)
        {
            SetLightGadget(player.SteamID, tool, extra, clear);
        }
        public void SetHeavyGadget(ulong steamID, string tool, int extra, bool clear = false)
        {
            using (var packet = Common.Serialization.Stream.Get())
            {
                packet.Write((byte)NetworkCommuncation.SetPlayerGadget);
                packet.Write(steamID);
                packet.Write((byte)4);//Tool A
                packet.Write(tool);
                packet.Write((byte)extra);
                packet.Write(clear);

                WriteToSocket(packet);
            }
        }
        public void SetHeavyGadget(Player<TPlayer> player, string tool, int extra, bool clear = false)
        {
            SetHeavyGadget(player.SteamID, tool, extra, clear);
        }
        public void SetThrowable(ulong steamID, string tool, int extra, bool clear = false)
        {
            using (var packet = Common.Serialization.Stream.Get())
            {
                packet.Write((byte)NetworkCommuncation.SetPlayerGadget);
                packet.Write(steamID);
                packet.Write((byte)5);//Tool A
                packet.Write(tool);
                packet.Write((byte)extra);
                packet.Write(clear);

                WriteToSocket(packet);
            }
        }
        public void SetThrowable(Player<TPlayer> player, string tool, int extra, bool clear = false)
        {
            SetThrowable(player.SteamID, tool, extra, clear);
        }

        // ---- Closing ----
        public void CloseConnection(string additionInfo = "")
        {
            if (string.IsNullOrWhiteSpace(additionInfo))
                this.mInternal.TerminationReason = additionInfo;
            else
                this.mInternal.TerminationReason = "User requested to terminate the connection";
            this.mInternal.mWantsToCloseConnection = true;
        }
        private void mClose(string reason)
        {
            if (this.IsConnected)
            {
                this.mInternal.TerminationReason = reason;
                this.mInternal.IsConnected = false;
            }
        }

        // ---- Disposing ----
        public void Dispose()
        {
            if (this.mInternal.Socket != null)
            {
                this.mInternal.Socket.SafeClose();
                this.mInternal.Socket = null;
            }
        }

        // ---- Overrides ----
        public override string ToString()
        {
            return
                this.GameIP + ":" + this.GamePort + " - " +
                this.ServerName;
        }

        // ---- Static ----
        public static TGameServer CreateInstance<TGameServer>(Internal @internal) where TGameServer : GameServer<TPlayer>
        {
            TGameServer gameServer = (TGameServer)Activator.CreateInstance(typeof(TGameServer));
            gameServer.mInternal = @internal;
            return gameServer;
        }

        // ---- Internal ----
        public class Internal
        {
            // ---- Variables ---- 
            public ulong ServerHash;
            public bool IsConnected;
            public IPAddress GameIP;
            public int GamePort;
            public TcpClient Socket;
            public Func<GameServer<TPlayer>, Internal, Common.Serialization.Stream, Task> mExecutionFunc;
            public bool IsPasswordProtected;
            public string ServerName;
            public string Gamemode;
            public string Map;
            public MapSize MapSize;
            public MapDayNight DayNight;
            public int CurrentPlayerCount;
            public int InQueuePlayerCount;
            public int MaxPlayerCount;
            public string LoadingScreenText;
            public string ServerRulesText;
            public ServerSettings<TPlayer> ServerSettings;
            public MapRotation<TPlayer> MapRotation;
            public GamemodeRotation<TPlayer> GamemodeRotation;
            public RoundSettings<TPlayer> RoundSettings;
            public string TerminationReason;
            public bool ReconnectFlag;

            // ---- Private Variables ---- 
            public byte[] mKeepAliveBuffer;
            public Common.Serialization.Stream mWriteStream;
            public Common.Serialization.Stream mReadStream;
            public uint mReadPackageSize;
            public long mLastPackageReceived;
            public long mLastPackageSent;
            public bool mWantsToCloseConnection;
            public StringBuilder mBuilder;

            public Internal()
            {
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
                this.mBuilder = new StringBuilder(4096);

                this.ServerSettings = new ServerSettings<TPlayer>(this);
                this.MapRotation = new MapRotation<TPlayer>(this);
                this.GamemodeRotation = new GamemodeRotation<TPlayer>(this);
                this.RoundSettings = new RoundSettings<TPlayer>(this);
            }

            // ---- Players In Room ---- 
            public Dictionary<ulong, Player<TPlayer>> Players = new Dictionary<ulong, Player<TPlayer>>(254);

            // ---- Room Settings ---- 
            public mRoomSettings _RoomSettings = new mRoomSettings();
            public bool IsDirtyRoomSettings;

            // ---- Round Settings ---- 
            public mRoundSettings _RoundSettings = new mRoundSettings();
            public bool IsDirtyRoundSettings;

            // ---- Map Rotation ---- 
            public HashSet<string> _MapRotation = new HashSet<string>(8);
            public bool IsDirtyMapRotation = false;

            // ---- Gamemode Rotation ---- 
            public HashSet<string> _GamemodeRotation = new HashSet<string>(8);
            public bool IsDirtyGamemodeRotation = false;

            // ---- Public Functions ---- 
            public void Set(Func<GameServer<TPlayer>, Internal, Common.Serialization.Stream, Task> func, TcpClient socket, IPAddress iP, int port, bool isPasswordProtected, string serverName, string gamemode, string map, MapSize mapSize, MapDayNight dayNight, int currentPlayers, int inQueuePlayers, int maxPlayers, string loadingScreenText, string serverRulesText)
            {
                this.ServerHash = ((ulong)port << 32) | (ulong)iP.ToUInt();
                this.IsConnected = true;
                this.GameIP = iP;
                this.GamePort = port;
                this.Socket = socket;
                this.mExecutionFunc = func;
                this.IsPasswordProtected = isPasswordProtected;
                this.ServerName = serverName;
                this.Gamemode = gamemode;
                this.Map = map;
                this.MapSize = mapSize;
                this.DayNight = dayNight;
                this.CurrentPlayerCount = currentPlayers;
                this.InQueuePlayerCount = inQueuePlayers;
                this.MaxPlayerCount = maxPlayers;
                this.LoadingScreenText = loadingScreenText;
                this.ServerRulesText = serverRulesText;

                this.ServerSettings.Reset();
                this._RoomSettings.Reset();
                this.IsDirtyRoomSettings = false;

                this.MapRotation.Reset();
                this._MapRotation.Clear();
                this.IsDirtyMapRotation = false;

                this.GamemodeRotation.Reset();
                this._GamemodeRotation.Clear();
                this.IsDirtyGamemodeRotation = false;

                this.RoundSettings.Reset();
                this._RoundSettings.Reset();
                this.IsDirtyRoundSettings = false;

                this.TerminationReason = string.Empty;
                this.ReconnectFlag = false;

                this.mWriteStream.Reset();
                this.mReadStream.Reset();
                this.mReadPackageSize = 0;
                this.mLastPackageReceived = Extentions.TickCount;
                this.mLastPackageSent = Extentions.TickCount;
                this.mWantsToCloseConnection = false;
                this.mBuilder.Clear();
            }
            public void AddPlayer(Player<TPlayer> player)
            {
                lock (Players)
                {
                    Players.Remove(player.SteamID);
                    Players.Add(player.SteamID, player);
                }
            }
            public void RemovePlayer<TPlayer>(TPlayer player) where TPlayer : Player<TPlayer>
            {
                lock (Players)
                    Players.Remove(player.SteamID);
            }
            public bool TryGetPlayer(ulong steamID, out Player<TPlayer> result)
            {
                lock (Players)
                    return Players.TryGetValue(steamID, out result);
            }
        }
        public class mRoomSettings
        {
            public float DamageMultiplier = 1.0f;
            public bool BleedingEnabled = true;
            public bool StaminaEnabled = false;
            public bool FriendlyFireEnabled = false;
            public bool HideMapVotes = true;
            public bool OnlyWinnerTeamCanVote = false;
            public bool HitMarkersEnabled = true;
            public bool PointLogEnabled = true;
            public bool SpectatorEnabled = true;
            public float CaptureFlagSpeedMultiplier = 1f;

            public byte MedicLimitPerSquad = 8;
            public byte EngineerLimitPerSquad = 8;
            public byte SupportLimitPerSquad = 8;
            public byte ReconLimitPerSquad = 8;

            public void Write(Common.Serialization.Stream ser)
            {
                ser.Write(this.DamageMultiplier);
                ser.Write(this.BleedingEnabled);
                ser.Write(this.StaminaEnabled);
                ser.Write(this.FriendlyFireEnabled);
                ser.Write(this.HideMapVotes);
                ser.Write(this.OnlyWinnerTeamCanVote);
                ser.Write(this.HitMarkersEnabled);
                ser.Write(this.PointLogEnabled);
                ser.Write(this.SpectatorEnabled);
                ser.Write(this.CaptureFlagSpeedMultiplier);

                ser.Write(this.MedicLimitPerSquad);
                ser.Write(this.EngineerLimitPerSquad);
                ser.Write(this.SupportLimitPerSquad);
                ser.Write(this.ReconLimitPerSquad);
            }
            public void Read(Common.Serialization.Stream ser)
            {
                this.DamageMultiplier = ser.ReadFloat();
                this.BleedingEnabled = ser.ReadBool();
                this.StaminaEnabled = ser.ReadBool();
                this.FriendlyFireEnabled = ser.ReadBool();
                this.HideMapVotes = ser.ReadBool();
                this.OnlyWinnerTeamCanVote = ser.ReadBool();
                this.HitMarkersEnabled = ser.ReadBool();
                this.PointLogEnabled = ser.ReadBool();
                this.SpectatorEnabled = ser.ReadBool();
                this.CaptureFlagSpeedMultiplier = ser.ReadFloat();

                this.MedicLimitPerSquad = ser.ReadInt8();
                this.EngineerLimitPerSquad = ser.ReadInt8();
                this.SupportLimitPerSquad = ser.ReadInt8();
                this.ReconLimitPerSquad = ser.ReadInt8();
            }
            public void Reset()
            {
                this.DamageMultiplier = 1.0f;
                this.BleedingEnabled = true;
                this.StaminaEnabled = false;
                this.FriendlyFireEnabled = false;
                this.HideMapVotes = true;
                this.OnlyWinnerTeamCanVote = false;
                this.HitMarkersEnabled = true;
                this.PointLogEnabled = true;
                this.SpectatorEnabled = true;

                this.MedicLimitPerSquad = 8;
                this.EngineerLimitPerSquad = 8;
                this.SupportLimitPerSquad = 8;
                this.ReconLimitPerSquad = 8;
            }
        }
        public class mRoundSettings
        {
            public const int Size = 1 + 8 + 8 + 8 + 4 + 4;

            public GameState State = GameState.WaitingForPlayers;
            public double TeamATickets = 0;
            public double TeamBTickets = 0;
            public double MaxTickets = 1;
            public int PlayersToStart = 16;
            public int SecondsLeft = 60;

            public void Write(Common.Serialization.Stream ser)
            {
                ser.Write((byte)this.State);
                ser.Write(this.TeamATickets);
                ser.Write(this.TeamBTickets);
                ser.Write(this.MaxTickets);
                ser.Write(this.PlayersToStart);
                ser.Write(this.SecondsLeft);
            }
            public void Read(Common.Serialization.Stream ser)
            {
                this.State = (GameState)ser.ReadInt8();
                this.TeamATickets = ser.ReadDouble();
                this.TeamBTickets = ser.ReadDouble();
                this.MaxTickets = ser.ReadDouble();
                this.PlayersToStart = ser.ReadInt32();
                this.SecondsLeft = ser.ReadInt32();
            }

            public void Reset()
            {
                this.State = GameState.WaitingForPlayers;
                this.TeamATickets = 0;
                this.TeamBTickets = 0;
                this.MaxTickets = 1;
                this.PlayersToStart = 16;
                this.SecondsLeft = 60;
            }
        }
    }
}
