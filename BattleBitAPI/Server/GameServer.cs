using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using BattleBitAPI.Common;
using BattleBitAPI.Common.Extentions;
using BattleBitAPI.Networking;
using BattleBitAPI.Pooling;

namespace BattleBitAPI.Server
{
    public class GameServer<TPlayer> : System.IDisposable where TPlayer : Player<TPlayer>
    {
        // ---- Public Variables ---- 
        public ulong ServerHash => mInternal.ServerHash;
        public bool IsConnected => mInternal.IsConnected;
        public IPAddress GameIP => mInternal.GameIP;
        public int GamePort => mInternal.GamePort;

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
        public uint RoundIndex => mInternal.RoundIndex;
        public long SessionID => mInternal.SessionID;
        public ServerSettings<TPlayer> ServerSettings => mInternal.ServerSettings;
        public MapRotation<TPlayer> MapRotation => mInternal.MapRotation;
        public GamemodeRotation<TPlayer> GamemodeRotation => mInternal.GamemodeRotation;
        public RoundSettings<TPlayer> RoundSettings => mInternal.RoundSettings;
        public string TerminationReason => mInternal.TerminationReason;
        public bool ReconnectFlag => mInternal.ReconnectFlag;
        public IEnumerable<Squad<TPlayer>> TeamASquads
        {
            get
            {
                for (int i = 1; i < this.mInternal.TeamASquads.Length; i++)
                    yield return this.mInternal.TeamASquads[i];
            }
        }
        public IEnumerable<Squad<TPlayer>> TeamBSquads
        {
            get
            {
                for (int i = 1; i < this.mInternal.TeamBSquads.Length; i++)
                    yield return this.mInternal.TeamBSquads[i];
            }
        }
        public IEnumerable<Squad<TPlayer>> AllSquads
        {
            get
            {
                for (int i = 1; i < this.mInternal.TeamASquads.Length; i++)
                    yield return this.mInternal.TeamASquads[i];
                for (int i = 1; i < this.mInternal.TeamBSquads.Length; i++)
                    yield return this.mInternal.TeamBSquads[i];
            }
        }

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

            //Gather all changes.
            this.mInternal.mChangedModifications.Clear();
            lock (this.mInternal.Players)
            {
                foreach (var steamid in this.mInternal.Players.Keys)
                {
                    var @internal = this.mInternal.mGetInternals(steamid);
                    if (@internal._Modifications.IsDirtyFlag)
                        this.mInternal.mChangedModifications.Enqueue((steamid, @internal._Modifications));
                }
            }

            //Send all changes.
            while (this.mInternal.mChangedModifications.Count > 0)
            {
                (ulong steamID, PlayerModifications<TPlayer>.mPlayerModifications modifications) item = this.mInternal.mChangedModifications.Dequeue();

                item.modifications.IsDirtyFlag = false;

                //Send new settings
                using (var pck = Common.Serialization.Stream.Get())
                {
                    pck.Write((byte)NetworkCommuncation.SetPlayerModifications);
                    pck.Write(item.steamID);
                    item.modifications.Write(pck);
                    WriteToSocket(pck);
                }
            }

            try
            {
                //Are we still connected on socket level?
                if (mInternal.Socket == null || !mInternal.Socket.Connected)
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

                var networkStream = mInternal.Socket.GetStream();

                //Read network packages.
                while (mInternal.Socket.Available > 0)
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
                using (var list = this.mInternal.PlayerPool.Get())
                {
                    //Get A copy of players to our list
                    lock (this.mInternal.Players)
                        list.ListItems.AddRange(this.mInternal.Players.Values);

                    //Iterate our list.
                    for (int i = 0; i < list.ListItems.Count; i++)
                        yield return (TPlayer)list.ListItems[i];
                }
            }
        }
        public IEnumerable<TPlayer> AllTeamAPlayers
        {
            get
            {
                using (var list = this.mInternal.PlayerPool.Get())
                {
                    //Get A copy of players to our list
                    lock (this.mInternal.Players)
                        list.ListItems.AddRange(this.mInternal.Players.Values);

                    //Iterate our list.
                    for (int i = 0; i < list.ListItems.Count; i++)
                    {
                        var item = list.ListItems[i];
                        if (item.Team == Team.TeamA)
                            yield return (TPlayer)item;
                    }
                }
            }
        }
        public IEnumerable<TPlayer> AllTeamBPlayers
        {
            get
            {
                using (var list = this.mInternal.PlayerPool.Get())
                {
                    //Get A copy of players to our list
                    lock (this.mInternal.Players)
                        list.ListItems.AddRange(this.mInternal.Players.Values);

                    //Iterate our list.
                    for (int i = 0; i < list.ListItems.Count; i++)
                    {
                        var item = list.ListItems[i];
                        if (item.Team == Team.TeamB)
                            yield return (TPlayer)item;
                    }
                }
            }
        }
        public IEnumerable<TPlayer> PlayersOf(Team team)
        {
            using (var list = this.mInternal.PlayerPool.Get())
            {
                //Get A copy of players to our list
                lock (this.mInternal.Players)
                    list.ListItems.AddRange(this.mInternal.Players.Values);

                //Iterate our list.
                for (int i = 0; i < list.ListItems.Count; i++)
                {
                    var item = list.ListItems[i];
                    if (item.Team == team)
                        yield return (TPlayer)item;
                }
            }
        }
        public IEnumerable<TPlayer> SearchPlayerByName(string keyword)
        {
            keyword = keyword.ToLower().Replace(" ", "");

            using (var list = this.mInternal.PlayerPool.Get())
            {
                //Get A copy of players to our list
                lock (this.mInternal.Players)
                    list.ListItems.AddRange(this.mInternal.Players.Values);

                //Iterate our list.
                for (int i = 0; i < list.ListItems.Count; i++)
                {
                    var item = list.ListItems[i];
                    if (item.Name.ToLower().Replace(" ", "").Contains(keyword))
                        yield return (TPlayer)item;
                }
            }
        }
        public IEnumerable<TPlayer> SearchPlayerByName(params string[] keywords)
        {
            for (int i = 0; i < keywords.Length; i++)
                keywords[i] = keywords[i].ToLower().Replace(" ", "");

            using (var list = this.mInternal.PlayerPool.Get())
            {
                //Get A copy of players to our list
                lock (this.mInternal.Players)
                    list.ListItems.AddRange(this.mInternal.Players.Values);

                //Iterate our list.
                for (int i = 0; i < list.ListItems.Count; i++)
                {
                    var item = list.ListItems[i];
                    var lowerName = item.Name.ToLower().Replace(" ", "");

                    for (int x = 0; x < keywords.Length; x++)
                    {
                        if (lowerName.Contains(keywords[x]))
                        {
                            yield return (TPlayer)item;
                            break;
                        }
                    }
                }
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
        public virtual async Task OnPlayerJoinedSquad(TPlayer player, Squad<TPlayer> squad)
        {

        }
        public virtual async Task OnPlayerLeftSquad(TPlayer player, Squad<TPlayer> squad)
        {

        }
        public virtual async Task OnPlayerChangeTeam(TPlayer player, Team team)
        {

        }
        public virtual async Task OnSquadPointsChanged(Squad<TPlayer> squad, int newPoints)
        {

        }
        public virtual async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(TPlayer player, OnPlayerSpawnArguments request)
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
        public virtual async Task OnSessionChanged(long oldSessionID, long newSessionID)
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
        public void SayToAllChat(string msg)
        {
            ExecuteCommand("say " + msg);
        }
        public void SayToChat(string msg, ulong steamID)
        {
            ExecuteCommand("sayto " + steamID + " " + msg);
        }
        public void SayToChat(string msg, Player<TPlayer> player)
        {
            SayToChat(msg, player.SteamID);
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
                SpawnProtection = spawnProtection,
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
        public void SetSquadPointsOf(Team team, Squads squad, int points)
        {
            ExecuteCommand("setsquadpoints " + ((int)(team)) + " " + ((int)squad) + " " + points);
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

        // ---- Squads ---- 
        public IEnumerable<TPlayer> IterateMembersOf(Squad<TPlayer> squad)
        {
            using (var list = this.mInternal.PlayerPool.Get())
            {
                var rsquad = this.mInternal.GetSquadInternal(squad);

                //Get A copy of players to our list
                lock (rsquad.Members)
                    list.ListItems.AddRange(rsquad.Members);

                //Iterate our list.
                for (int i = 0; i < list.ListItems.Count; i++)
                    yield return (TPlayer)list.ListItems[i];
            }
        }
        public Squad<TPlayer> GetSquad(Team team, Squads name)
        {
            if (team == Team.TeamA)
                return this.mInternal.TeamASquads[(int)name];
            if (team == Team.TeamB)
                return this.mInternal.TeamBSquads[(int)name];
            return null;
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
        internal static void SetInstance(GameServer<TPlayer> server, Internal @internal)
        {
            server.mInternal = @internal;
        }

        // ---- Internal ----
        public class Internal
        {
            // ---- Variables ---- 
            public ulong ServerHash;
            public bool IsConnected;
            public bool HasActiveConnectionSession;
            public IPAddress GameIP;
            public int GamePort;
            public TcpClient Socket;
            public Func<GameServer<TPlayer>, Internal, Common.Serialization.Stream, Task> mExecutionFunc;
            public Func<ulong, Player<TPlayer>.Internal> mGetInternals;
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
            public uint RoundIndex;
            public long SessionID;
            public ServerSettings<TPlayer> ServerSettings;
            public MapRotation<TPlayer> MapRotation;
            public GamemodeRotation<TPlayer> GamemodeRotation;
            public RoundSettings<TPlayer> RoundSettings;
            public string TerminationReason;
            public bool ReconnectFlag;
            public Squad<TPlayer>.Internal[] TeamASquadInternals;
            public Squad<TPlayer>.Internal[] TeamBSquadInternals;
            public Squad<TPlayer>[] TeamASquads;
            public Squad<TPlayer>[] TeamBSquads;
            public ItemPooling<Player<TPlayer>> PlayerPool;

            // ---- Private Variables ---- 
            public byte[] mKeepAliveBuffer;
            public Common.Serialization.Stream mWriteStream;
            public Common.Serialization.Stream mReadStream;
            public uint mReadPackageSize;
            public long mLastPackageReceived;
            public long mLastPackageSent;
            public bool mWantsToCloseConnection;
            public long mPreviousSessionID;
            public StringBuilder mBuilder;
            public Queue<(ulong steamID, PlayerModifications<TPlayer>.mPlayerModifications)> mChangedModifications;

            public Internal(GameServer<TPlayer> server)
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
                this.mChangedModifications = new Queue<(ulong steamID, PlayerModifications<TPlayer>.mPlayerModifications)>(254);

                this.TeamASquadInternals = new Squad<TPlayer>.Internal[]
                {
                    null,
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Alpha),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Bravo),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Charlie),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Delta ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Echo ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Foxtrot ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Golf ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Hotel ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.India),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Juliett ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Kilo ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Lima ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Mike ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.November),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Oscar ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Papa ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Quebec),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Romeo ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Sierra),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Tango ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Uniform ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Whiskey ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Xray ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Yankee ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Zulu ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Ash ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Baker ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Cast ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Diver),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Eagle),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Fisher),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.George),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Hanover),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Ice ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Jake),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.King),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Lash),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Mule),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Neptune ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Ostend),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Page ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Quail ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Raft ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Scout ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Tare ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Unit ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.William ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Xaintrie ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Yoke ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Zebra ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Ace ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Beer ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Cast2 ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Duff ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Edward ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Freddy),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Gustav),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Henry ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Ivar ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Jazz ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Key ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Lincoln ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Mary ),
                    new Squad<TPlayer>.Internal(server,Team.TeamA, Squads.Nora ),
                };
                this.TeamBSquadInternals = new Squad<TPlayer>.Internal[]
                {
                    null,
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Alpha),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Bravo),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Charlie),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Delta ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Echo ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Foxtrot ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Golf ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Hotel ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.India),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Juliett ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Kilo ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Lima ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Mike ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.November),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Oscar ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Papa ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Quebec),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Romeo ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Sierra),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Tango ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Uniform ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Whiskey ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Xray ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Yankee ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Zulu ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Ash ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Baker ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Cast ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Diver),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Eagle),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Fisher),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.George),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Hanover),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Ice ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Jake),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.King),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Lash),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Mule),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Neptune ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Ostend),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Page ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Quail ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Raft ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Scout ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Tare ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Unit ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.William ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Xaintrie ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Yoke ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Zebra ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Ace ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Beer ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Cast2 ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Duff ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Edward ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Freddy),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Gustav),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Henry ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Ivar ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Jazz ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Key ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Lincoln ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Mary ),
                    new Squad<TPlayer>.Internal(server,Team.TeamB, Squads.Nora ),
                };

                this.TeamASquads = new Squad<TPlayer>[]
                {
                    null,
                    new Squad<TPlayer>(this.TeamASquadInternals[01]),
                    new Squad<TPlayer>(this.TeamASquadInternals[02]),
                    new Squad<TPlayer>(this.TeamASquadInternals[03]),
                    new Squad<TPlayer>(this.TeamASquadInternals[04]),
                    new Squad<TPlayer>(this.TeamASquadInternals[05]),
                    new Squad<TPlayer>(this.TeamASquadInternals[06]),
                    new Squad<TPlayer>(this.TeamASquadInternals[07]),
                    new Squad<TPlayer>(this.TeamASquadInternals[08]),
                    new Squad<TPlayer>(this.TeamASquadInternals[09]),
                    new Squad<TPlayer>(this.TeamASquadInternals[10]),
                    new Squad<TPlayer>(this.TeamASquadInternals[11]),
                    new Squad<TPlayer>(this.TeamASquadInternals[12]),
                    new Squad<TPlayer>(this.TeamASquadInternals[13]),
                    new Squad<TPlayer>(this.TeamASquadInternals[14]),
                    new Squad<TPlayer>(this.TeamASquadInternals[15]),
                    new Squad<TPlayer>(this.TeamASquadInternals[16]),
                    new Squad<TPlayer>(this.TeamASquadInternals[17]),
                    new Squad<TPlayer>(this.TeamASquadInternals[18]),
                    new Squad<TPlayer>(this.TeamASquadInternals[19]),
                    new Squad<TPlayer>(this.TeamASquadInternals[20]),
                    new Squad<TPlayer>(this.TeamASquadInternals[21]),
                    new Squad<TPlayer>(this.TeamASquadInternals[22]),
                    new Squad<TPlayer>(this.TeamASquadInternals[23]),
                    new Squad<TPlayer>(this.TeamASquadInternals[24]),
                    new Squad<TPlayer>(this.TeamASquadInternals[25]),
                    new Squad<TPlayer>(this.TeamASquadInternals[26]),
                    new Squad<TPlayer>(this.TeamASquadInternals[27]),
                    new Squad<TPlayer>(this.TeamASquadInternals[28]),
                    new Squad<TPlayer>(this.TeamASquadInternals[29]),
                    new Squad<TPlayer>(this.TeamASquadInternals[30]),
                    new Squad<TPlayer>(this.TeamASquadInternals[31]),
                    new Squad<TPlayer>(this.TeamASquadInternals[32]),
                    new Squad<TPlayer>(this.TeamASquadInternals[33]),
                    new Squad<TPlayer>(this.TeamASquadInternals[34]),
                    new Squad<TPlayer>(this.TeamASquadInternals[35]),
                    new Squad<TPlayer>(this.TeamASquadInternals[36]),
                    new Squad<TPlayer>(this.TeamASquadInternals[37]),
                    new Squad<TPlayer>(this.TeamASquadInternals[38]),
                    new Squad<TPlayer>(this.TeamASquadInternals[39]),
                    new Squad<TPlayer>(this.TeamASquadInternals[40]),
                    new Squad<TPlayer>(this.TeamASquadInternals[41]),
                    new Squad<TPlayer>(this.TeamASquadInternals[42]),
                    new Squad<TPlayer>(this.TeamASquadInternals[43]),
                    new Squad<TPlayer>(this.TeamASquadInternals[44]),
                    new Squad<TPlayer>(this.TeamASquadInternals[45]),
                    new Squad<TPlayer>(this.TeamASquadInternals[46]),
                    new Squad<TPlayer>(this.TeamASquadInternals[47]),
                    new Squad<TPlayer>(this.TeamASquadInternals[48]),
                    new Squad<TPlayer>(this.TeamASquadInternals[49]),
                    new Squad<TPlayer>(this.TeamASquadInternals[50]),
                    new Squad<TPlayer>(this.TeamASquadInternals[51]),
                    new Squad<TPlayer>(this.TeamASquadInternals[52]),
                    new Squad<TPlayer>(this.TeamASquadInternals[53]),
                    new Squad<TPlayer>(this.TeamASquadInternals[54]),
                    new Squad<TPlayer>(this.TeamASquadInternals[55]),
                    new Squad<TPlayer>(this.TeamASquadInternals[56]),
                    new Squad<TPlayer>(this.TeamASquadInternals[57]),
                    new Squad<TPlayer>(this.TeamASquadInternals[58]),
                    new Squad<TPlayer>(this.TeamASquadInternals[59]),
                    new Squad<TPlayer>(this.TeamASquadInternals[60]),
                    new Squad<TPlayer>(this.TeamASquadInternals[61]),
                    new Squad<TPlayer>(this.TeamASquadInternals[62]),
                    new Squad<TPlayer>(this.TeamASquadInternals[63]),
                    new Squad<TPlayer>(this.TeamASquadInternals[64]),
                };
                this.TeamBSquads = new Squad<TPlayer>[]
                {
                    null,
                    new Squad<TPlayer>(this.TeamBSquadInternals[01]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[02]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[03]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[04]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[05]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[06]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[07]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[08]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[09]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[10]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[11]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[12]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[13]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[14]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[15]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[16]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[17]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[18]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[19]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[20]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[21]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[22]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[23]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[24]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[25]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[26]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[27]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[28]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[29]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[30]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[31]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[32]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[33]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[34]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[35]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[36]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[37]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[38]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[39]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[40]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[41]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[42]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[43]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[44]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[45]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[46]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[47]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[48]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[49]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[50]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[51]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[52]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[53]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[54]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[55]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[56]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[57]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[58]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[59]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[60]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[61]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[62]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[63]),
                    new Squad<TPlayer>(this.TeamBSquadInternals[64]),
                };
                this.PlayerPool = new ItemPooling<Player<TPlayer>>(254);
            }

            // ---- Players In Room ---- 
            public Dictionary<ulong, Player<TPlayer>> Players = new Dictionary<ulong, Player<TPlayer>>(254);

            // ---- Room Settings ---- 
            public ServerSettings<TPlayer>.mRoomSettings _RoomSettings = new ServerSettings<TPlayer>.mRoomSettings();
            public bool IsDirtyRoomSettings;

            // ---- Round Settings ---- 
            public RoundSettings<TPlayer>.mRoundSettings _RoundSettings = new RoundSettings<TPlayer>.mRoundSettings();
            public bool IsDirtyRoundSettings;

            // ---- Map Rotation ---- 
            public HashSet<string> _MapRotation = new HashSet<string>(8);
            public bool IsDirtyMapRotation = false;

            // ---- Gamemode Rotation ---- 
            public HashSet<string> _GamemodeRotation = new HashSet<string>(8);
            public bool IsDirtyGamemodeRotation = false;

            // ---- Public Functions ---- 
            public void Set(
                Func<GameServer<TPlayer>, Internal, Common.Serialization.Stream, Task> func,
                Func<ulong, Player<TPlayer>.Internal> internalGetFunc,
                TcpClient socket,
                IPAddress iP,
                int port,
                bool isPasswordProtected,
                string serverName,
                string gamemode,
                string map,
                MapSize mapSize,
                MapDayNight dayNight,
                int currentPlayers,
                int inQueuePlayers,
                int maxPlayers,
                string loadingScreenText,
                string serverRulesText,
                uint roundIndex,
                long sessionID
                )
            {
                this.ServerHash = ((ulong)port << 32) | (ulong)iP.ToUInt();
                this.IsConnected = true;
                this.GameIP = iP;
                this.GamePort = port;
                this.Socket = socket;
                this.mExecutionFunc = func;
                this.mGetInternals = internalGetFunc;
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
                this.RoundIndex = roundIndex;
                this.SessionID = sessionID;

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
                this.mChangedModifications.Clear();
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
            public Squad<TPlayer>.Internal GetSquadInternal(Team team, Squads squad)
            {
                if (team == Team.TeamA)
                    return this.TeamASquadInternals[(int)squad];
                if (team == Team.TeamB)
                    return this.TeamBSquadInternals[(int)squad];
                return null;
            }
            public Squad<TPlayer>.Internal GetSquadInternal(Squad<TPlayer> squad)
            {
                return GetSquadInternal(squad.Team, squad.Name);
            }
        }
    }
}
