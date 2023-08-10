using System.Net;
using System.Net.Sockets;
using System.Numerics;
using BattleBitAPI.Common;
using BattleBitAPI.Common.Extentions;
using BattleBitAPI.Common.Serialization;
using BattleBitAPI.Networking;
using CommunityServerAPI.BattleBitAPI;

namespace BattleBitAPI.Server
{
    public class ServerListener<TPlayer> : IDisposable where TPlayer : Player
    {
        // --- Public --- 
        public bool IsListening { get; private set; }
        public bool IsDisposed { get; private set; }
        public int ListeningPort { get; private set; }

        // --- Events --- 
        /// <summary>
        /// Fired when game server is ticking (~100hz)<br/>
        /// </summary>
        public Func<GameServer, Task> OnGameServerTick { get; set; }

        /// <summary>
        /// Fired when an attempt made to connect to the server.<br/>
        /// Default, any connection attempt will be accepted
        /// </summary>
        /// 
        /// <remarks>
        /// IPAddress: IP of incoming connection <br/>
        /// </remarks>
        /// 
        /// <value>
        /// Returns: true if allow connection, false if deny the connection.
        /// </value>
        public Func<IPAddress, Task<bool>> OnGameServerConnecting { get; set; }

        /// <summary>
        /// Fired when a game server connects.
        /// </summary>
        /// 
        /// <remarks>
        /// GameServer: Game server that is connecting.<br/>
        /// </remarks>
        public Func<GameServer, Task> OnGameServerConnected { get; set; }

        /// <summary>
        /// Fired when a game server reconnects. (When game server connects while a socket is already open)
        /// </summary>
        /// 
        /// <remarks>
        /// GameServer: Game server that is reconnecting.<br/>
        /// </remarks>
        public Func<GameServer, Task> OnGameServerReconnected { get; set; }

        /// <summary>
        /// Fired when a game server disconnects. Check (GameServer.TerminationReason) to see the reason.
        /// </summary>
        /// 
        /// <remarks>
        /// GameServer: Game server that disconnected.<br/>
        /// </remarks>
        public Func<GameServer, Task> OnGameServerDisconnected { get; set; }

        /// <summary>
        /// Fired when a player connects to a server.<br/>
        /// Check player.GameServer get the server that player joined.
        /// </summary>
        /// 
        /// <remarks>
        /// Player: The player that connected to the server<br/>
        /// </remarks>
        public Func<TPlayer, Task> OnPlayerConnected { get; set; }

        /// <summary>
        /// Fired when a player disconnects from a server.<br/>
        /// Check player.GameServer get the server that player left.
        /// </summary>
        /// 
        /// <remarks>
        /// Player: The player that disconnected from the server<br/>
        /// </remarks>
        public Func<TPlayer, Task> OnPlayerDisconnected { get; set; }

        /// <summary>
        /// Fired when a player types a message to text chat.<br/>
        /// </summary>
        /// 
        /// <remarks>
        /// Player: The player that typed the message <br/>
        /// ChatChannel: The channel the message was sent <br/>
        /// string - Message: The message<br/>
        /// </remarks>
        /// <value>
        /// Returns: True if you let the message broadcasted, false if you don't it to be broadcasted.
        /// </value>
        public Func<TPlayer, ChatChannel, string, Task<bool>> OnPlayerTypedMessage { get; set; }

        /// <summary>
        /// Fired when a player kills another player.
        /// </summary>
        /// 
        /// <remarks>
        /// OnPlayerKillArguments: Details about the kill<br/>
        /// </remarks>
        public Func<OnPlayerKillArguments<TPlayer>, Task> OnAPlayerKilledAnotherPlayer { get; set; }

        /// <summary>
        /// Fired when game server requests the stats of a player, this function should return in 3000ms or player will not able to join to server.
        /// </summary>
        /// 
        /// <remarks>
        /// ulong - SteamID of the player<br/>
        /// PlayerStats - The official stats of the player<br/>
        /// </remarks>
        /// <value>
        /// Returns: The modified stats of the player.
        /// </value>
        public Func<ulong, PlayerStats, Task<PlayerStats>> OnGetPlayerStats { get; set; }

        /// <summary>
        /// Fired when game server requests to save the stats of a player.
        /// </summary>
        /// 
        /// <remarks>
        /// ulong - SteamID of the player<br/>
        /// PlayerStats - Stats of the player<br/>
        /// </remarks>
        /// <value>
        /// Returns: The stats of the player.
        /// </value>
        public Func<ulong, PlayerStats, Task> OnSavePlayerStats { get; set; }

        /// <summary>
        /// Fired when a player requests server to change role.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player requesting<br/>
        /// GameRole - The role the player asking to change<br/>
        /// </remarks>
        /// <value>
        /// Returns: True if you accept if, false if you don't.
        /// </value>
        public Func<TPlayer, GameRole, Task<bool>> OnPlayerRequestingToChangeRole { get; set; }

        /// <summary>
        /// Fired when a player changes their game role.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player<br/>
        /// GameRole - The new role of the player<br/>
        /// </remarks>
        public Func<TPlayer, GameRole, Task> OnPlayerChangedRole { get; set; }

        /// <summary>
        /// Fired when a player joins a squad.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player<br/>
        /// Squads - The squad player joined<br/>
        /// </remarks>
        public Func<TPlayer, Squads, Task> OnPlayerJoinedASquad { get; set; }

        /// <summary>
        /// Fired when a player leaves their squad.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player<br/>
        /// Squads - The squad that player left<br/>
        /// </remarks>
        public Func<TPlayer, Squads, Task> OnPlayerLeftSquad { get; set; }

        /// <summary>
        /// Fired when a player changes team.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player<br/>
        /// Team - The new team that player joined<br/>
        /// </remarks>
        public Func<TPlayer, Team, Task> OnPlayerChangedTeam { get; set; }

        /// <summary>
        /// Fired when a player is spawning.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player<br/>
        /// PlayerSpawnRequest - The request<br/>
        /// </remarks>
        /// <value>
        /// Returns: The new spawn response
        /// </value>
        public Func<TPlayer, PlayerSpawnRequest, Task<PlayerSpawnRequest>> OnPlayerSpawning { get; set; }

        /// <summary>
        /// Fired when a player is spawns
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player<br/>
        /// </remarks>
        public Func<TPlayer, Task> OnPlayerSpawned { get; set; }

        /// <summary>
        /// Fired when a player dies
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The player<br/>
        /// </remarks>
        public Func<TPlayer, Task> OnPlayerDied { get; set; }

        /// <summary>
        /// Fired when a player reports another player.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer - The reporter player<br/>
        /// TPlayer - The reported player<br/>
        /// ReportReason - The reason of report<br/>
        /// String - Additional detail<br/>
        /// </remarks>
        public Func<TPlayer, TPlayer, ReportReason, string, Task> OnPlayerReported { get; set; }

        // --- Private --- 
        private TcpListener mSocket;
        private Dictionary<ulong, GameServer> mActiveConnections;

        // --- Construction --- 
        public ServerListener()
        {
            this.mActiveConnections = new Dictionary<ulong, GameServer>(16);
        }

        // --- Starting ---
        public void Start(IPAddress bindIP, int port)
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (bindIP == null)
                throw new ArgumentNullException(nameof(bindIP));
            if (IsListening)
                throw new Exception("Server is already listening.");

            this.mSocket = new TcpListener(bindIP, port);
            this.mSocket.Start();

            this.ListeningPort = port;
            this.IsListening = true;

            mMainLoop();
        }
        public void Start(int port)
        {
            Start(IPAddress.Loopback, port);
        }

        // --- Stopping ---
        public void Stop()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (!IsListening)
                throw new Exception("Already not running.");

            try
            {
                mSocket.Stop();
            }
            catch { }

            this.mSocket = null;
            this.ListeningPort = 0;
            this.IsListening = true;
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

            bool allow = true;
            if (OnGameServerConnecting != null)
                allow = await OnGameServerConnecting(ip);

            if (!allow)
            {
                //Connection is not allowed from this IP.
                client.SafeClose();
                return;
            }

            GameServer server = null;
            GameServer.mInternalResources resources;
            try
            {
                using (CancellationTokenSource source = new CancellationTokenSource(Const.HailConnectTimeout))
                {
                    using (var readStream = Common.Serialization.Stream.Get())
                    {
                        var networkStream = client.GetStream();

                        //Read package type
                        {
                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, 1, source.Token))
                                throw new Exception("Unable to read the package type");
                            NetworkCommuncation type = (NetworkCommuncation)readStream.ReadInt8();
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

                        resources = new GameServer.mInternalResources();
                        server = new GameServer(client, resources, mExecutePackage, ip, gamePort, isPasswordProtected, serverName, gameMode, gamemap, size, dayNight, currentPlayers, queuePlayers, maxPlayers, loadingScreenText, serverRulesText);

                        //Room settings
                        {
                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, 4, source.Token))
                                throw new Exception("Unable to read the room size");
                            int roomSize = (int)readStream.ReadUInt32();

                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, roomSize, source.Token))
                                throw new Exception("Unable to read the room");
                            resources.Settings.Read(readStream);
                        }

                        //Map&gamemode rotation
                        {
                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, 4, source.Token))
                                throw new Exception("Unable to read the map&gamemode rotation size");
                            int rotationSize = (int)readStream.ReadUInt32();

                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, rotationSize, source.Token))
                                throw new Exception("Unable to read the map&gamemode");

                            uint count = readStream.ReadUInt32();
                            while (count > 0)
                            {
                                count--;
                                if (readStream.TryReadString(out var item))
                                    resources.MapRotation.Add(item.ToUpperInvariant());
                            }

                            count = readStream.ReadUInt32();
                            while (count > 0)
                            {
                                count--;
                                if (readStream.TryReadString(out var item))
                                    resources.GamemodeRotation.Add(item);
                            }
                        }

                        //Client Count
                        int clientCount = 0;
                        {
                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, 1, source.Token))
                                throw new Exception("Unable to read the Client Count Players");
                            clientCount = readStream.ReadInt8();
                        }

                        //Get each client.
                        while (clientCount > 0)
                        {
                            clientCount--;

                            ulong steamid = 0;
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 8, source.Token))
                                    throw new Exception("Unable to read the SteamId");
                                steamid = readStream.ReadUInt64();
                            }

                            string username;
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 2, source.Token))
                                    throw new Exception("Unable to read the Username Size");

                                int stringSize = readStream.ReadUInt16();
                                if (stringSize > 0)
                                {
                                    readStream.Reset();
                                    if (!await networkStream.TryRead(readStream, stringSize, source.Token))
                                        throw new Exception("Unable to read the Username");

                                    username = readStream.ReadString(stringSize);
                                }
                                else
                                {
                                    username = string.Empty;
                                }
                            }

                            //Team
                            Team team;
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 1, source.Token))
                                    throw new Exception("Unable to read the Team");
                                team = (Team)readStream.ReadInt8();
                            }

                            //Squad
                            Squads squad;
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 1, source.Token))
                                    throw new Exception("Unable to read the Squad");
                                squad = (Squads)readStream.ReadInt8();
                            }

                            //Role
                            GameRole role;
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 1, source.Token))
                                    throw new Exception("Unable to read the Role");
                                role = (GameRole)readStream.ReadInt8();
                            }

                            var loadout = new PlayerLoadout();
                            var wearings = new PlayerWearings();

                            //IsAlive
                            bool isAlive;
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 1, source.Token))
                                    throw new Exception("Unable to read the isAlive");
                                isAlive = readStream.ReadBool();
                            }

                            //Loadout + Wearings
                            if (isAlive)
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 4, source.Token))
                                    throw new Exception("Unable to read the LoadoutSize");
                                int loadoutSize = (int)readStream.ReadUInt32();

                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, loadoutSize, source.Token))
                                    throw new Exception("Unable to read the Loadout + Wearings");
                                loadout.Read(readStream);
                                wearings.Read(readStream);
                            }

                            TPlayer player = Activator.CreateInstance<TPlayer>();
                            player.SteamID = steamid;
                            player.Name = username;
                            player.GameServer = server;
                            player.Team = team;
                            player.Squad = squad;
                            player.Role = role;
                            player.IsAlive = isAlive;
                            player.CurrentLoadout = loadout;
                            player.CurrentWearings = wearings;

                            await player.OnInitialized();

                            resources.AddPlayer(player);
                        }

                        //Send accepted notification.
                        networkStream.WriteByte((byte)NetworkCommuncation.Accepted);
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    var networkStream = client.GetStream();
                    using (var pck = BattleBitAPI.Common.Serialization.Stream.Get())
                    {
                        pck.Write((byte)NetworkCommuncation.Denied);
                        pck.Write(e.Message);

                        //Send denied notification.
                        networkStream.Write(pck.Buffer, 0, pck.WritePosition);
                    }
                    await networkStream.FlushAsync();
                }
                catch { }

                if (server != null)
                {
                    server.Dispose();
                    server = null;
                }

                client.SafeClose();
                return;
            }

            bool connectionExist = false;

            //Track the connection
            lock (this.mActiveConnections)
            {
                //An old connection exist with same IP + Port?
                if (connectionExist = this.mActiveConnections.TryGetValue(server.ServerHash, out var oldServer))
                {
                    oldServer.ReconnectFlag = true;
                    this.mActiveConnections.Remove(server.ServerHash);
                }

                this.mActiveConnections.Add(server.ServerHash, server);
            }

            //Call the callback.
            if (!connectionExist)
            {
                //New connection!
                if (OnGameServerConnected != null)
                    await OnGameServerConnected.Invoke(server);
            }
            else
            {
                //Reconnection
                if (OnGameServerReconnected != null)
                    await OnGameServerReconnected.Invoke(server);
            }

            //Set the buffer sizes.
            client.ReceiveBufferSize = Const.MaxNetworkPackageSize;
            client.SendBufferSize = Const.MaxNetworkPackageSize;

            //Join to main server loop.
            await mHandleGameServer(server);
        }
        private async Task mHandleGameServer(GameServer server)
        {
            using (server)
            {
                while (server.IsConnected)
                {
                    if (OnGameServerTick != null)
                        await OnGameServerTick(server);
                    await server.Tick();
                    await Task.Delay(10);
                }

                if (OnGameServerDisconnected != null && !server.ReconnectFlag)
                    await OnGameServerDisconnected.Invoke(server);
            }

            //Remove from list.
            if (!server.ReconnectFlag)
                lock (this.mActiveConnections)
                    this.mActiveConnections.Remove(server.ServerHash);
        }

        // --- Logic Executing ---
        private async Task mExecutePackage(GameServer server, GameServer.mInternalResources resources, Common.Serialization.Stream stream)
        {
            var communcation = (NetworkCommuncation)stream.ReadInt8();
            switch (communcation)
            {
                case NetworkCommuncation.PlayerConnected:
                    {
                        if (stream.CanRead(8 + 2 + (1 + 1 + 1)))
                        {
                            ulong steamID = stream.ReadUInt64();
                            if (stream.TryReadString(out var username))
                            {
                                Team team = (Team)stream.ReadInt8();
                                Squads squad = (Squads)stream.ReadInt8();
                                GameRole role = (GameRole)stream.ReadInt8();

                                TPlayer player = Activator.CreateInstance<TPlayer>();
                                player.SteamID = steamID;
                                player.Name = username;
                                player.GameServer = server;

                                player.Team = team;
                                player.Squad = squad;
                                player.Role = role;

                                await player.OnInitialized();

                                resources.AddPlayer(player);
                                if (OnPlayerConnected != null)
                                    await OnPlayerConnected.Invoke(player);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.PlayerDisconnected:
                    {
                        if (stream.CanRead(8))
                        {
                            ulong steamID = stream.ReadUInt64();
                            bool exist;
                            Player player;
                            lock (resources.Players)
                                exist = resources.Players.Remove(steamID, out player);

                            if (exist)
                            {
                                if (OnPlayerDisconnected != null)
                                    await OnPlayerDisconnected.Invoke((TPlayer)player);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerTypedMessage:
                    {
                        if (stream.CanRead(2 + 8 + 1 + 2))
                        {
                            ushort messageID = stream.ReadUInt16();
                            ulong steamID = stream.ReadUInt64();

                            if (resources.TryGetPlayer(steamID, out var player))
                            {
                                ChatChannel chat = (ChatChannel)stream.ReadInt8();
                                if (stream.TryReadString(out var msg))
                                {
                                    bool pass = true;
                                    if (OnPlayerTypedMessage != null)
                                        pass = await OnPlayerTypedMessage.Invoke((TPlayer)player, chat, msg);

                                    //Respond back.
                                    using (var response = Common.Serialization.Stream.Get())
                                    {
                                        response.Write((byte)NetworkCommuncation.RespondPlayerMessage);
                                        response.Write(messageID);
                                        response.Write(pass);
                                        server.WriteToSocket(response);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerKilledAnotherPlayer:
                    {
                        if (stream.CanRead(8 + 12 + 8 + 12 + 2 + 1 + 1))
                        {
                            ulong killer = stream.ReadUInt64();
                            Vector3 killerPos = new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());

                            ulong victim = stream.ReadUInt64();
                            Vector3 victimPos = new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());

                            if (stream.TryReadString(out var tool))
                            {
                                PlayerBody body = (PlayerBody)stream.ReadInt8();
                                ReasonOfDamage source = (ReasonOfDamage)stream.ReadInt8();

                                if (resources.TryGetPlayer(killer, out var killerClient))
                                {
                                    if (resources.TryGetPlayer(victim, out var victimClient))
                                    {
                                        if (OnAPlayerKilledAnotherPlayer != null)
                                        {
                                            var args = new OnPlayerKillArguments<TPlayer>()
                                            {
                                                Killer = (TPlayer)killerClient,
                                                KillerPosition = killerPos,
                                                Victim = (TPlayer)victimClient,
                                                VictimPosition = victimPos,
                                                BodyPart = body,
                                                SourceOfDamage = source,
                                                KillerTool = tool,
                                            };

                                            await OnAPlayerKilledAnotherPlayer.Invoke(args);

                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
                case NetworkCommuncation.GetPlayerStats:
                    {
                        if (stream.CanRead(8 + 2))
                        {
                            ulong steamID = stream.ReadUInt64();

                            var stats = new PlayerStats();
                            stats.Read(stream);

                            if (OnGetPlayerStats != null)
                                stats = await OnGetPlayerStats.Invoke(steamID, stats);

                            using (var response = Common.Serialization.Stream.Get())
                            {
                                response.Write((byte)NetworkCommuncation.SendPlayerStats);
                                response.Write(steamID);
                                stats.Write(response);
                                server.WriteToSocket(response);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.SavePlayerStats:
                    {
                        if (stream.CanRead(8 + 4))
                        {
                            ulong steamID = stream.ReadUInt64();
                            PlayerStats stats = new PlayerStats();
                            stats.Read(stream);

                            if (OnSavePlayerStats != null)
                                await OnSavePlayerStats.Invoke(steamID, stats);
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerAskingToChangeRole:
                    {
                        if (stream.CanRead(8 + 1))
                        {
                            ulong steamID = stream.ReadUInt64();
                            GameRole role = (GameRole)stream.ReadInt8();

                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                bool accepted = true;

                                if (OnPlayerRequestingToChangeRole != null)
                                    accepted = await OnPlayerRequestingToChangeRole.Invoke((TPlayer)client, role);

                                if (accepted)
                                    server.SetRoleTo(steamID, role);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerChangedRole:
                    {
                        if (stream.CanRead(8 + 1))
                        {
                            ulong steamID = stream.ReadUInt64();
                            GameRole role = (GameRole)stream.ReadInt8();

                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                client.Role = role;
                                if (OnPlayerChangedRole != null)
                                    await OnPlayerChangedRole.Invoke((TPlayer)client, role);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerJoinedASquad:
                    {
                        if (stream.CanRead(8 + 1))
                        {
                            ulong steamID = stream.ReadUInt64();
                            Squads squad = (Squads)stream.ReadInt8();

                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                client.Squad = squad;
                                if (OnPlayerJoinedASquad != null)
                                    await OnPlayerJoinedASquad.Invoke((TPlayer)client, squad);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerLeftSquad:
                    {
                        if (stream.CanRead(8))
                        {
                            ulong steamID = stream.ReadUInt64();

                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                var oldSquad = client.Squad;
                                var oldRole = client.Role;
                                client.Squad = Squads.NoSquad;
                                client.Role = GameRole.Assault;

                                if (OnPlayerLeftSquad != null)
                                    await OnPlayerLeftSquad.Invoke((TPlayer)client, oldSquad);

                                if (oldRole != GameRole.Assault)
                                    if (OnPlayerChangedRole != null)
                                        await OnPlayerChangedRole.Invoke((TPlayer)client, GameRole.Assault);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerChangedTeam:
                    {
                        if (stream.CanRead(8 + 1))
                        {
                            ulong steamID = stream.ReadUInt64();
                            Team team = (Team)stream.ReadInt8();

                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                client.Team = team;
                                if (OnPlayerChangedTeam != null)
                                    await OnPlayerChangedTeam.Invoke((TPlayer)client, team);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerRequestingToSpawn:
                    {
                        if (stream.CanRead(2))
                        {
                            ulong steamID = stream.ReadUInt64();

                            var request = new PlayerSpawnRequest();
                            request.Read(stream);
                            ushort vehicleID = stream.ReadUInt16();

                            if (resources.TryGetPlayer(steamID, out var client))
                                if (this.OnPlayerSpawning != null)
                                    request = await OnPlayerSpawning.Invoke((TPlayer)client, request);

                            //Respond back.
                            using (var response = Common.Serialization.Stream.Get())
                            {
                                response.Write((byte)NetworkCommuncation.SpawnPlayer);
                                response.Write(steamID);
                                request.Write(response);
                                response.Write(vehicleID);
                                server.WriteToSocket(response);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerReport:
                    {
                        if (stream.CanRead(8 + 8 + 1 + 2))
                        {
                            ulong reporter = stream.ReadUInt64();
                            ulong reported = stream.ReadUInt64();
                            ReportReason reason = (ReportReason)stream.ReadInt8();
                            stream.TryReadString(out var additionalInfo);

                            if (resources.TryGetPlayer(reporter, out var reporterClient))
                            {
                                if (resources.TryGetPlayer(reported, out var reportedClient))
                                {
                                    if (OnPlayerReported != null)
                                        await OnPlayerReported.Invoke((TPlayer)reporterClient, (TPlayer)reportedClient, reason, additionalInfo);
                                }
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerSpawn:
                    {
                        if (stream.CanRead(8 + 2))
                        {
                            ulong reporter = stream.ReadUInt64();
                            if (resources.TryGetPlayer(reporter, out var client))
                            {
                                var loadout = new PlayerLoadout();
                                loadout.Read(stream);
                                client.CurrentLoadout = loadout;

                                var wearings = new PlayerWearings();
                                wearings.Read(stream);
                                client.CurrentWearings = wearings;

                                client.IsAlive = true;

                                await client.OnSpawned();

                                if (OnPlayerSpawned != null)
                                    await OnPlayerSpawned.Invoke((TPlayer)client);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerDie:
                    {
                        if (stream.CanRead(8))
                        {
                            ulong reporter = stream.ReadUInt64();
                            if (resources.TryGetPlayer(reporter, out var client))
                            {
                                client.CurrentLoadout = new PlayerLoadout();
                                client.CurrentWearings = new PlayerWearings();
                                client.IsAlive = false;

                                await client.OnDied();

                                if (OnPlayerDied != null)
                                    await OnPlayerDied.Invoke((TPlayer)client);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.NotifyNewMapRotation:
                    {
                        if (stream.CanRead(4))
                        {
                            uint count = stream.ReadUInt32();
                            lock (resources.MapRotation)
                            {
                                resources.MapRotation.Clear();
                                while (count > 0)
                                {
                                    count--;
                                    if (stream.TryReadString(out var map))
                                        resources.MapRotation.Add(map.ToUpperInvariant());
                                }
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.NotifyNewGamemodeRotation:
                    {
                        if (stream.CanRead(4))
                        {
                            uint count = stream.ReadUInt32();
                            lock (resources.GamemodeRotation)
                            {
                                resources.GamemodeRotation.Clear();
                                while (count > 0)
                                {
                                    count--;
                                    if (stream.TryReadString(out var map))
                                        resources.GamemodeRotation.Add(map);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        // --- Disposing --- 
        public void Dispose()
        {
            //Already disposed?
            if (this.IsDisposed)
                return;
            this.IsDisposed = true;

            if (IsListening)
                Stop();
        }
    }
}
