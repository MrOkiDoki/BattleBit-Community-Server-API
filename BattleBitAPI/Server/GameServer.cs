using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using BattleBitAPI.Common;
using BattleBitAPI.Common.Extentions;
using BattleBitAPI.Networking;
using CommunityServerAPI.BattleBitAPI;
using Stream = BattleBitAPI.Common.Serialization.Stream;

namespace BattleBitAPI.Server;

public class GameServer<TPlayer> : IDisposable where TPlayer : Player<TPlayer>
{
    // ---- Private Variables ---- 
    private Internal mInternal;

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

    // ---- Team ----
    public IEnumerable<TPlayer> AllPlayers
    {
        get
        {
            var list = new List<TPlayer>(mInternal.Players.Values.Count);
            lock (mInternal.Players)
            {
                foreach (var item in mInternal.Players.Values)
                    list.Add((TPlayer)item);
            }

            return list;
        }
    }

    // ---- Disposing ----
    public void Dispose()
    {
        if (mInternal.Socket != null)
        {
            mInternal.Socket.SafeClose();
            mInternal.Socket = null;
        }
    }

    // ---- Tick ----
    public async Task Tick()
    {
        if (!IsConnected)
            return;

        if (mInternal.IsDirtyRoomSettings)
        {
            mInternal.IsDirtyRoomSettings = false;

            //Send new settings
            using (var pck = Stream.Get())
            {
                pck.Write((byte)NetworkCommunication.SetNewRoomSettings);
                mInternal._RoomSettings.Write(pck);
                WriteToSocket(pck);
            }
        }

        if (mInternal.IsDirtyMapRotation)
        {
            mInternal.IsDirtyMapRotation = false;
            mInternal.mBuilder.Clear();

            mInternal.mBuilder.Append("setmaprotation ");
            lock (mInternal._MapRotation)
            {
                foreach (var map in mInternal._MapRotation)
                {
                    mInternal.mBuilder.Append(map);
                    mInternal.mBuilder.Append(',');
                }
            }

            ExecuteCommand(mInternal.mBuilder.ToString());
        }

        if (mInternal.IsDirtyGamemodeRotation)
        {
            mInternal.IsDirtyGamemodeRotation = false;
            mInternal.mBuilder.Clear();

            mInternal.mBuilder.Append("setgamemoderotation ");
            lock (mInternal._GamemodeRotation)
            {
                foreach (var gamemode in mInternal._GamemodeRotation)
                {
                    mInternal.mBuilder.Append(gamemode);
                    mInternal.mBuilder.Append(',');
                }
            }

            ExecuteCommand(mInternal.mBuilder.ToString());
        }

        if (mInternal.IsDirtyRoundSettings)
        {
            mInternal.IsDirtyRoundSettings = false;

            //Send new round settings
            using (var pck = Stream.Get())
            {
                pck.Write((byte)NetworkCommunication.SetNewRoundState);
                mInternal._RoundSettings.Write(pck);
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
            if (mInternal.mWantsToCloseConnection)
            {
                mClose(TerminationReason);
                return;
            }

            var networkStream = Socket.GetStream();

            //Read network packages.
            while (Socket.Available > 0)
            {
                mInternal.mLastPackageReceived = Extentions.TickCount;

                //Do we know the package size?
                if (mInternal.mReadPackageSize == 0)
                {
                    const int sizeToRead = 4;
                    var leftSizeToRead = sizeToRead - mInternal.mReadStream.WritePosition;

                    var read = await networkStream.ReadAsync(mInternal.mReadStream.Buffer, mInternal.mReadStream.WritePosition, leftSizeToRead);
                    if (read <= 0)
                        throw new Exception("Connection was terminated.");

                    mInternal.mReadStream.WritePosition += read;

                    //Did we receive the package?
                    if (mInternal.mReadStream.WritePosition >= 4)
                    {
                        //Read the package size
                        mInternal.mReadPackageSize = mInternal.mReadStream.ReadUInt32();

                        if (mInternal.mReadPackageSize > Const.MaxNetworkPackageSize)
                            throw new Exception("Incoming package was larger than 'Conts.MaxNetworkPackageSize'");

                        mInternal.mReadStream.Reset();
                    }
                }
                else
                {
                    var sizeToRead = (int)mInternal.mReadPackageSize;
                    var leftSizeToRead = sizeToRead - mInternal.mReadStream.WritePosition;

                    var read = await networkStream.ReadAsync(mInternal.mReadStream.Buffer, mInternal.mReadStream.WritePosition, leftSizeToRead);
                    if (read <= 0)
                        throw new Exception("Connection was terminated.");

                    mInternal.mReadStream.WritePosition += read;

                    //Do we have the package?
                    if (mInternal.mReadStream.WritePosition >= mInternal.mReadPackageSize)
                    {
                        mInternal.mReadPackageSize = 0;

                        await mInternal.mExecutionFunc(this, mInternal, mInternal.mReadStream);

                        //Reset
                        mInternal.mReadStream.Reset();
                    }
                }
            }

            //Send the network packages.
            if (mInternal.mWriteStream.WritePosition > 0)
                lock (mInternal.mWriteStream)
                {
                    if (mInternal.mWriteStream.WritePosition > 0)
                    {
                        networkStream.WriteAsync(mInternal.mWriteStream.Buffer, 0, mInternal.mWriteStream.WritePosition);
                        mInternal.mWriteStream.WritePosition = 0;

                        mInternal.mLastPackageSent = Extentions.TickCount;
                    }
                }

            //Are we timed out?
            if (Extentions.TickCount - mInternal.mLastPackageReceived > Const.NetworkTimeout)
                throw new Exception("Timedout.");

            //Send keep alive if needed
            if (Extentions.TickCount - mInternal.mLastPackageSent > Const.NetworkKeepAlive)
            {
                //Send keep alive.
                await networkStream.WriteAsync(mInternal.mKeepAliveBuffer, 0, 4);
                await networkStream.FlushAsync();
                mInternal.mLastPackageSent = Extentions.TickCount;
            }
        }
        catch (Exception e)
        {
            mClose(e.Message);
        }
    }

    public bool TryGetPlayer(ulong steamID, out TPlayer player)
    {
        lock (mInternal.Players)
        {
            if (mInternal.Players.TryGetValue(steamID, out var _player))
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

    public virtual async Task OnAPlayerRevivedAnotherPlayer(TPlayer from, TPlayer to)
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
    public void WriteToSocket(Stream pck)
    {
        lock (mInternal.mWriteStream)
        {
            mInternal.mWriteStream.Write((uint)pck.WritePosition);
            mInternal.mWriteStream.Write(pck.Buffer, 0, pck.WritePosition);
        }
    }

    public void ExecuteCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd))
            return;

        var bytesLong = Encoding.UTF8.GetByteCount(cmd);
        lock (mInternal.mWriteStream)
        {
            mInternal.mWriteStream.Write((uint)(1 + 2 + bytesLong));
            mInternal.mWriteStream.Write((byte)NetworkCommunication.ExecuteCommand);
            mInternal.mWriteStream.Write(cmd);
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
        ExecuteCommand("setsquad " + steamID + " " + (int)targetSquad);
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
        var request = new OnPlayerSpawnArguments
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
        using (var response = Stream.Get())
        {
            response.Write((byte)NetworkCommunication.SpawnPlayer);
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
        using (var packet = Stream.Get())
        {
            packet.Write((byte)NetworkCommunication.SetPlayerWeapon);
            packet.Write(steamID);
            packet.Write((byte)0); //Primary
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
        using (var packet = Stream.Get())
        {
            packet.Write((byte)NetworkCommunication.SetPlayerWeapon);
            packet.Write(steamID);
            packet.Write((byte)1); //Secondary
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
        using (var packet = Stream.Get())
        {
            packet.Write((byte)NetworkCommunication.SetPlayerGadget);
            packet.Write(steamID);
            packet.Write((byte)2); //first aid
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
        using (var packet = Stream.Get())
        {
            packet.Write((byte)NetworkCommunication.SetPlayerGadget);
            packet.Write(steamID);
            packet.Write((byte)3); //Tool A
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
        using (var packet = Stream.Get())
        {
            packet.Write((byte)NetworkCommunication.SetPlayerGadget);
            packet.Write(steamID);
            packet.Write((byte)4); //Tool A
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
        using (var packet = Stream.Get())
        {
            packet.Write((byte)NetworkCommunication.SetPlayerGadget);
            packet.Write(steamID);
            packet.Write((byte)5); //Tool A
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
            mInternal.TerminationReason = additionInfo;
        else
            mInternal.TerminationReason = "User requested to terminate the connection";
        mInternal.mWantsToCloseConnection = true;
    }

    private void mClose(string reason)
    {
        if (IsConnected)
        {
            mInternal.TerminationReason = reason;
            mInternal.IsConnected = false;
        }
    }

    // ---- Overrides ----
    public override string ToString()
    {
        return
            GameIP + ":" + GamePort + " - " +
            ServerName;
    }

    // ---- Static ----
    public static TGameServer CreateInstance<TGameServer>(Internal @internal) where TGameServer : GameServer<TPlayer>
    {
        var gameServer = (TGameServer)Activator.CreateInstance(typeof(TGameServer));
        gameServer.mInternal = @internal;
        return gameServer;
    }

    // ---- Internal ----
    public class Internal
    {
        // ---- Gamemode Rotation ---- 
        public HashSet<string> _GamemodeRotation = new(8);

        // ---- Map Rotation ---- 
        public HashSet<string> _MapRotation = new(8);

        // ---- Room Settings ---- 
        public mRoomSettings _RoomSettings = new();

        // ---- Round Settings ---- 
        public mRoundSettings _RoundSettings = new();
        public int CurrentPlayerCount;
        public MapDayNight DayNight;
        public IPAddress GameIP;
        public string Gamemode;
        public GamemodeRotation<TPlayer> GamemodeRotation;
        public int GamePort;
        public int InQueuePlayerCount;
        public bool IsConnected;
        public bool IsDirtyGamemodeRotation;
        public bool IsDirtyMapRotation;
        public bool IsDirtyRoomSettings;
        public bool IsDirtyRoundSettings;
        public bool IsPasswordProtected;
        public string LoadingScreenText;
        public string Map;
        public MapRotation<TPlayer> MapRotation;
        public MapSize MapSize;
        public int MaxPlayerCount;
        public StringBuilder mBuilder;
        public Func<GameServer<TPlayer>, Internal, Stream, Task> mExecutionFunc;

        // ---- Private Variables ---- 
        public byte[] mKeepAliveBuffer;
        public long mLastPackageReceived;
        public long mLastPackageSent;
        public uint mReadPackageSize;
        public Stream mReadStream;
        public bool mWantsToCloseConnection;
        public Stream mWriteStream;

        // ---- Players In Room ---- 
        public Dictionary<ulong, Player<TPlayer>> Players = new(254);
        public bool ReconnectFlag;

        public RoundSettings<TPlayer> RoundSettings;

        // ---- Variables ---- 
        public ulong ServerHash;
        public string ServerName;
        public string ServerRulesText;
        public ServerSettings<TPlayer> ServerSettings;
        public TcpClient Socket;
        public string TerminationReason;

        public Internal()
        {
            TerminationReason = string.Empty;
            mWriteStream = new Stream
            {
                Buffer = new byte[Const.MaxNetworkPackageSize],
                InPool = false,
                ReadPosition = 0,
                WritePosition = 0
            };
            mReadStream = new Stream
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
            mLastPackageReceived = Extentions.TickCount;
            mLastPackageSent = Extentions.TickCount;
            mBuilder = new StringBuilder(4096);

            ServerSettings = new ServerSettings<TPlayer>(this);
            MapRotation = new MapRotation<TPlayer>(this);
            GamemodeRotation = new GamemodeRotation<TPlayer>(this);
            RoundSettings = new RoundSettings<TPlayer>(this);
        }

        // ---- Public Functions ---- 
        public void Set(Func<GameServer<TPlayer>, Internal, Stream, Task> func, TcpClient socket, IPAddress iP, int port, bool isPasswordProtected, string serverName, string gamemode, string map, MapSize mapSize, MapDayNight dayNight, int currentPlayers, int inQueuePlayers, int maxPlayers, string loadingScreenText, string serverRulesText)
        {
            ServerHash = ((ulong)port << 32) | iP.ToUInt();
            IsConnected = true;
            GameIP = iP;
            GamePort = port;
            Socket = socket;
            mExecutionFunc = func;
            IsPasswordProtected = isPasswordProtected;
            ServerName = serverName;
            Gamemode = gamemode;
            Map = map;
            MapSize = mapSize;
            DayNight = dayNight;
            CurrentPlayerCount = currentPlayers;
            InQueuePlayerCount = inQueuePlayers;
            MaxPlayerCount = maxPlayers;
            LoadingScreenText = loadingScreenText;
            ServerRulesText = serverRulesText;

            ServerSettings.Reset();
            _RoomSettings.Reset();
            IsDirtyRoomSettings = false;

            MapRotation.Reset();
            _MapRotation.Clear();
            IsDirtyMapRotation = false;

            GamemodeRotation.Reset();
            _GamemodeRotation.Clear();
            IsDirtyGamemodeRotation = false;

            RoundSettings.Reset();
            _RoundSettings.Reset();
            IsDirtyRoundSettings = false;

            TerminationReason = string.Empty;
            ReconnectFlag = false;

            mWriteStream.Reset();
            mReadStream.Reset();
            mReadPackageSize = 0;
            mLastPackageReceived = Extentions.TickCount;
            mLastPackageSent = Extentions.TickCount;
            mWantsToCloseConnection = false;
            mBuilder.Clear();
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
            {
                Players.Remove(player.SteamID);
            }
        }

        public bool TryGetPlayer(ulong steamID, out Player<TPlayer> result)
        {
            lock (Players)
            {
                return Players.TryGetValue(steamID, out result);
            }
        }
    }

    public class mRoomSettings
    {
        public bool BleedingEnabled = true;
        public float CaptureFlagSpeedMultiplier = 1f;
        public float DamageMultiplier = 1.0f;
        public byte EngineerLimitPerSquad = 8;
        public bool FriendlyFireEnabled;
        public bool HideMapVotes = true;
        public bool HitMarkersEnabled = true;

        public byte MedicLimitPerSquad = 8;
        public bool OnlyWinnerTeamCanVote;
        public bool PointLogEnabled = true;
        public byte ReconLimitPerSquad = 8;
        public bool SpectatorEnabled = true;
        public bool StaminaEnabled;
        public byte SupportLimitPerSquad = 8;

        public void Write(Stream ser)
        {
            ser.Write(DamageMultiplier);
            ser.Write(BleedingEnabled);
            ser.Write(StaminaEnabled);
            ser.Write(FriendlyFireEnabled);
            ser.Write(HideMapVotes);
            ser.Write(OnlyWinnerTeamCanVote);
            ser.Write(HitMarkersEnabled);
            ser.Write(PointLogEnabled);
            ser.Write(SpectatorEnabled);
            ser.Write(CaptureFlagSpeedMultiplier);

            ser.Write(MedicLimitPerSquad);
            ser.Write(EngineerLimitPerSquad);
            ser.Write(SupportLimitPerSquad);
            ser.Write(ReconLimitPerSquad);
        }

        public void Read(Stream ser)
        {
            DamageMultiplier = ser.ReadFloat();
            BleedingEnabled = ser.ReadBool();
            StaminaEnabled = ser.ReadBool();
            FriendlyFireEnabled = ser.ReadBool();
            HideMapVotes = ser.ReadBool();
            OnlyWinnerTeamCanVote = ser.ReadBool();
            HitMarkersEnabled = ser.ReadBool();
            PointLogEnabled = ser.ReadBool();
            SpectatorEnabled = ser.ReadBool();
            CaptureFlagSpeedMultiplier = ser.ReadFloat();

            MedicLimitPerSquad = ser.ReadInt8();
            EngineerLimitPerSquad = ser.ReadInt8();
            SupportLimitPerSquad = ser.ReadInt8();
            ReconLimitPerSquad = ser.ReadInt8();
        }

        public void Reset()
        {
            DamageMultiplier = 1.0f;
            BleedingEnabled = true;
            StaminaEnabled = false;
            FriendlyFireEnabled = false;
            HideMapVotes = true;
            OnlyWinnerTeamCanVote = false;
            HitMarkersEnabled = true;
            PointLogEnabled = true;
            SpectatorEnabled = true;

            MedicLimitPerSquad = 8;
            EngineerLimitPerSquad = 8;
            SupportLimitPerSquad = 8;
            ReconLimitPerSquad = 8;
        }
    }

    public class mRoundSettings
    {
        public const int Size = 1 + 8 + 8 + 8 + 4 + 4;
        public double MaxTickets = 1;
        public int PlayersToStart = 16;
        public int SecondsLeft = 60;

        public GameState State = GameState.WaitingForPlayers;
        public double TeamATickets;
        public double TeamBTickets;

        public void Write(Stream ser)
        {
            ser.Write((byte)State);
            ser.Write(TeamATickets);
            ser.Write(TeamBTickets);
            ser.Write(MaxTickets);
            ser.Write(PlayersToStart);
            ser.Write(SecondsLeft);
        }

        public void Read(Stream ser)
        {
            State = (GameState)ser.ReadInt8();
            TeamATickets = ser.ReadDouble();
            TeamBTickets = ser.ReadDouble();
            MaxTickets = ser.ReadDouble();
            PlayersToStart = ser.ReadInt32();
            SecondsLeft = ser.ReadInt32();
        }

        public void Reset()
        {
            State = GameState.WaitingForPlayers;
            TeamATickets = 0;
            TeamBTickets = 0;
            MaxTickets = 1;
            PlayersToStart = 16;
            SecondsLeft = 60;
        }
    }
}