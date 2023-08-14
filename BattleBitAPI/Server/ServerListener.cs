using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Xml;
using BattleBitAPI.Common;
using BattleBitAPI.Common.Extentions;
using BattleBitAPI.Common.Serialization;
using BattleBitAPI.Networking;
using CommunityServerAPI.BattleBitAPI;

namespace BattleBitAPI.Server
{
    public class ServerListener<TPlayer, TGameServer> : IDisposable where TPlayer : Player<TPlayer> where TGameServer : GameServer<TPlayer>
    {
        // --- Public --- 
        public bool IsListening { get; private set; }
        public bool IsDisposed { get; private set; }
        public int ListeningPort { get; private set; }

        // --- Events --- 
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
        public Func<GameServer<TPlayer>, Task> OnGameServerConnected { get; set; }

        /// <summary>
        /// Fired when a game server reconnects. (When game server connects while a socket is already open)
        /// </summary>
        /// 
        /// <remarks>
        /// GameServer: Game server that is reconnecting.<br/>
        /// </remarks>
        public Func<GameServer<TPlayer>, Task> OnGameServerReconnected { get; set; }

        /// <summary>
        /// Fired when a game server disconnects. Check (GameServer.TerminationReason) to see the reason.
        /// </summary>
        /// 
        /// <remarks>
        /// GameServer: Game server that disconnected.<br/>
        /// </remarks>
        public Func<GameServer<TPlayer>, Task> OnGameServerDisconnected { get; set; }

        /// <summary>
        /// Fired when a new instance of game server created.
        /// </summary>
        /// 
        /// <remarks>
        /// GameServer: Game server that has been just created.<br/>
        /// </remarks>
        public Func<GameServer<TPlayer>, Task> OnCreatingGameServerInstance { get; set; }

        /// <summary>
        /// Fired when a new instance of player instance created.
        /// </summary>
        /// 
        /// <remarks>
        /// TPlayer: The player instance that was created<br/>
        /// </remarks>
        public Func<TPlayer, Task> OnCreatingPlayerInstance { get; set; }

        // --- Private --- 
        private TcpListener mSocket;
        private Dictionary<ulong, (TGameServer server, GameServer<TPlayer>.Internal resources)> mActiveConnections;
        private mInstances<TPlayer, TGameServer> mInstanceDatabase;

        // --- Construction --- 
        public ServerListener()
        {
            this.mActiveConnections = new Dictionary<ulong, (TGameServer, GameServer<TPlayer>.Internal)>(16);
            this.mInstanceDatabase = new mInstances<TPlayer, TGameServer>();
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
            Start(IPAddress.Any, port);
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

            TGameServer server = null;
            GameServer<TPlayer>.Internal resources;
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

                        //Read is server protected
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

                        var hash = ((ulong)gamePort << 32) | (ulong)ip.ToUInt();
                        server = this.mInstanceDatabase.GetServerInstance(hash, out bool isNew, out resources);
                        resources.Set(
                            this.mExecutePackage,
                            client,
                            ip,
                            gamePort,
                            isPasswordProtected,
                            serverName,
                            gameMode,
                            gamemap,
                            size,
                            dayNight,
                            currentPlayers,
                            queuePlayers,
                            maxPlayers,
                            loadingScreenText,
                            serverRulesText
                            );

                        //Room settings
                        {
                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, 4, source.Token))
                                throw new Exception("Unable to read the room size");
                            int roomSize = (int)readStream.ReadUInt32();

                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, roomSize, source.Token))
                                throw new Exception("Unable to read the room");
                            resources._RoomSettings.Read(readStream);
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
                                    resources._MapRotation.Add(item.ToUpperInvariant());
                            }

                            count = readStream.ReadUInt32();
                            while (count > 0)
                            {
                                count--;
                                if (readStream.TryReadString(out var item))
                                    resources._GamemodeRotation.Add(item);
                            }
                        }

                        //Round Settings
                        {
                            readStream.Reset();
                            if (!await networkStream.TryRead(readStream, GameServer<TPlayer>.mRoundSettings.Size, source.Token))
                                throw new Exception("Unable to read the round settings");
                            resources._RoundSettings.Read(readStream);
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

                            uint ipHash = 0;
                            {
                                readStream.Reset();
                                if (!await networkStream.TryRead(readStream, 4, source.Token))
                                    throw new Exception("Unable to read the ip");
                                ipHash = readStream.ReadUInt32();
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

                            TPlayer player = mInstanceDatabase.GetPlayerInstance(steamid, out bool isNewClient, out var playerInternal);
                            playerInternal.SteamID = steamid;
                            playerInternal.Name = username;
                            playerInternal.IP = new IPAddress(ipHash);
                            playerInternal.GameServer = (GameServer<TPlayer>)server;
                            playerInternal.Team = team;
                            playerInternal.Squad = squad;
                            playerInternal.Role = role;
                            playerInternal.IsAlive = isAlive;
                            playerInternal.CurrentLoadout = loadout;
                            playerInternal.CurrentWearings = wearings;

                            if (isNewClient)
                            {
                                if (this.OnCreatingPlayerInstance != null)
                                    this.OnCreatingPlayerInstance(player);
                            }


                            resources.AddPlayer(player);
                        }

                        //Send accepted notification.
                        networkStream.WriteByte((byte)NetworkCommuncation.Accepted);

                        if (isNew)
                        {
                            if (this.OnCreatingGameServerInstance != null)
                                this.OnCreatingGameServerInstance(server);
                        }

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
                    oldServer.resources.ReconnectFlag = true;
                    this.mActiveConnections.Remove(server.ServerHash);
                }

                this.mActiveConnections.Add(server.ServerHash, (server, resources));
            }

            //Call the callback.
            if (!connectionExist)
            {
                //New connection!
                server.OnConnected();
                if (this.OnGameServerConnected != null)
                    this.OnGameServerConnected(server);
            }
            else
            {
                //Reconnection
                server.OnReconnected();
                if (this.OnGameServerReconnected != null)
                    this.OnGameServerReconnected(server);
            }

            //Set the buffer sizes.
            client.ReceiveBufferSize = Const.MaxNetworkPackageSize;
            client.SendBufferSize = Const.MaxNetworkPackageSize;

            //Join to main server loop.
            await mHandleGameServer(server, resources);
        }
        private async Task mHandleGameServer(TGameServer server, GameServer<TPlayer>.Internal @internal)
        {
            bool isTicking = false;

            using (server)
            {
                async Task mTickAsync()
                {
                    isTicking = true;
                    await server.OnTick();
                    isTicking = false;
                }

                while (server.IsConnected)
                {
                    if (!isTicking)
                        mTickAsync();

                    await server.Tick();
                    await Task.Delay(10);
                }

                if (!server.ReconnectFlag)
                {
                    server.OnDisconnected();

                    if (this.OnGameServerDisconnected != null)
                        this.OnGameServerDisconnected(server);
                }
            }

            //Remove from list.
            if (!server.ReconnectFlag)
                lock (this.mActiveConnections)
                    this.mActiveConnections.Remove(server.ServerHash);
        }

        // --- Logic Executing ---
        private async Task mExecutePackage(GameServer<TPlayer> server, GameServer<TPlayer>.Internal resources, Common.Serialization.Stream stream)
        {
            var communcation = (NetworkCommuncation)stream.ReadInt8();
            switch (communcation)
            {
                case NetworkCommuncation.PlayerConnected:
                    {
                        if (stream.CanRead(8 + 2 + 4 + (1 + 1 + 1)))
                        {
                            ulong steamID = stream.ReadUInt64();
                            if (stream.TryReadString(out var username))
                            {
                                uint ip = stream.ReadUInt32();
                                Team team = (Team)stream.ReadInt8();
                                Squads squad = (Squads)stream.ReadInt8();
                                GameRole role = (GameRole)stream.ReadInt8();

                                TPlayer player = mInstanceDatabase.GetPlayerInstance(steamID, out bool isNewClient, out var playerInternal);
                                playerInternal.SteamID = steamID;
                                playerInternal.Name = username;
                                playerInternal.IP = new IPAddress(ip);
                                playerInternal.GameServer = (GameServer<TPlayer>)server;

                                playerInternal.Team = team;
                                playerInternal.Squad = squad;
                                playerInternal.Role = role;

                                if (isNewClient)
                                {
                                    if (this.OnCreatingPlayerInstance != null)
                                        this.OnCreatingPlayerInstance(player);
                                }

                                resources.AddPlayer(player);
                                player.OnConnected();
                                server.OnPlayerConnected(player);
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
                            Player<TPlayer> player;
                            lock (resources.Players)
                                exist = resources.Players.Remove(steamID, out player);

                            if (exist)
                            {
                                var @internal = mInstanceDatabase.GetPlayerInternals(steamID);
                                if (@internal.HP > -1f)
                                {
                                    @internal.OnDie();

                                    player.OnDied();
                                    server.OnPlayerDied((TPlayer)player);
                                }

                                player.OnDisconnected();
                                server.OnPlayerDisconnected((TPlayer)player);
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
                                    async Task Handle()
                                    {
                                        var pass = await server.OnPlayerTypedMessage((TPlayer)player, chat, msg);

                                        //Respond back.
                                        using (var response = Common.Serialization.Stream.Get())
                                        {
                                            response.Write((byte)NetworkCommuncation.RespondPlayerMessage);
                                            response.Write(messageID);
                                            response.Write(pass);
                                            server.WriteToSocket(response);
                                        }
                                    }

                                    Handle();

                                }
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnAPlayerDownedAnotherPlayer:
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

                                        victimClient.OnDowned();
                                        server.OnAPlayerDownedAnotherPlayer(args);
                                    }
                                }
                            }
                        }

                        break;
                    }
                case NetworkCommuncation.OnPlayerJoining:
                    {
                        if (stream.CanRead(8 + 2))
                        {
                            ulong steamID = stream.ReadUInt64();
                            var stats = new PlayerStats();
                            stats.Read(stream);


                            async Task mHandle()
                            {
                                var args = new PlayerJoiningArguments()
                                {
                                    Stats = stats,
                                    Squad = Squads.NoSquad,
                                    Team = Team.None
                                };

                                await server.OnPlayerJoiningToServer(steamID, args);
                                using (var response = Common.Serialization.Stream.Get())
                                {
                                    response.Write((byte)NetworkCommuncation.SendPlayerStats);
                                    response.Write(steamID);
                                    args.Write(response);
                                    server.WriteToSocket(response);
                                }
                            }

                            mHandle();
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

                            server.OnSavePlayerStats(steamID, stats);
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
                                async Task mHandle()
                                {
                                    bool accepted = await server.OnPlayerRequestingToChangeRole((TPlayer)client, role);
                                    if (accepted)
                                        server.SetRoleTo(steamID, role);
                                }

                                mHandle();
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
                                var @internal = mInstanceDatabase.GetPlayerInternals(steamID);
                                @internal.Role = role;

                                client.OnChangedRole(role);
                                server.OnPlayerChangedRole((TPlayer)client, role);
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
                                var @internal = mInstanceDatabase.GetPlayerInternals(steamID);
                                @internal.Squad = squad;

                                client.OnJoinedSquad(squad);
                                server.OnPlayerJoinedSquad((TPlayer)client, squad);
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
                                var @internal = mInstanceDatabase.GetPlayerInternals(steamID);

                                var oldSquad = client.Squad;
                                var oldRole = client.Role;
                                @internal.Squad = Squads.NoSquad;
                                @internal.Role = GameRole.Assault;

                                client.OnLeftSquad(oldSquad);
                                server.OnPlayerLeftSquad((TPlayer)client, oldSquad);

                                if (oldRole != GameRole.Assault)
                                {
                                    client.OnChangedRole(GameRole.Assault);
                                    server.OnPlayerChangedRole((TPlayer)client, GameRole.Assault);
                                }
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
                                var @internal = mInstanceDatabase.GetPlayerInternals(steamID);

                                @internal.Team = team;

                                client.OnChangedTeam();
                                server.OnPlayerChangeTeam((TPlayer)client, team);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerRequestingToSpawn:
                    {
                        if (stream.CanRead(2))
                        {
                            ulong steamID = stream.ReadUInt64();

                            var request = new OnPlayerSpawnArguments();
                            request.Read(stream);
                            ushort vehicleID = stream.ReadUInt16();

                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                async Task mHandle()
                                {
                                    request = await server.OnPlayerSpawning((TPlayer)client, request);

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

                                mHandle();
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
                                    server.OnPlayerReported((TPlayer)reporterClient, (TPlayer)reportedClient, reason, additionalInfo);
                                }
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerSpawn:
                    {
                        if (stream.CanRead(8 + 2))
                        {
                            ulong steamID = stream.ReadUInt64();
                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                var @internal = mInstanceDatabase.GetPlayerInternals(steamID);

                                var loadout = new PlayerLoadout();
                                loadout.Read(stream);
                                @internal.CurrentLoadout = loadout;

                                var wearings = new PlayerWearings();
                                wearings.Read(stream);
                                @internal.CurrentWearings = wearings;

                                @internal.IsAlive = true;

                                client.OnSpawned();
                                server.OnPlayerSpawned((TPlayer)client);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerDie:
                    {
                        if (stream.CanRead(8))
                        {
                            ulong steamid = stream.ReadUInt64();
                            if (resources.TryGetPlayer(steamid, out var client))
                            {
                                var @internal = mInstanceDatabase.GetPlayerInternals(steamid);
                                @internal.OnDie();

                                client.OnDied();
                                server.OnPlayerDied((TPlayer)client);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.NotifyNewMapRotation:
                    {
                        if (stream.CanRead(4))
                        {
                            uint count = stream.ReadUInt32();
                            lock (resources._MapRotation)
                            {
                                resources._MapRotation.Clear();
                                while (count > 0)
                                {
                                    count--;
                                    if (stream.TryReadString(out var map))
                                        resources._MapRotation.Add(map.ToUpperInvariant());
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
                            lock (resources._GamemodeRotation)
                            {
                                resources._GamemodeRotation.Clear();
                                while (count > 0)
                                {
                                    count--;
                                    if (stream.TryReadString(out var map))
                                        resources._GamemodeRotation.Add(map);
                                }
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.NotifyNewRoundState:
                    {
                        if (stream.CanRead(GameServer<TPlayer>.mRoundSettings.Size))
                        {
                            var oldState = resources._RoundSettings.State;
                            resources._RoundSettings.Read(stream);
                            var newState = resources._RoundSettings.State;

                            if (newState != oldState)
                            {
                                server.OnGameStateChanged(oldState, newState);

                                if (newState == GameState.Playing)
                                    server.OnRoundStarted();
                                else if (newState == GameState.EndingGame)
                                    server.OnRoundEnded();
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerAskingToChangeTeam:
                    {
                        if (stream.CanRead(8 + 1))
                        {
                            ulong steamID = stream.ReadUInt64();
                            Team team = (Team)stream.ReadInt8();

                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                async Task mHandle()
                                {
                                    bool accepted = await server.OnPlayerRequestingToChangeTeam((TPlayer)client, team);
                                    if (accepted)
                                        server.ChangeTeam(steamID, team);
                                }

                                mHandle();
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.GameTick:
                    {
                        if (stream.CanRead(4 + 4 + 4))
                        {
                            float decompressX = stream.ReadFloat();
                            float decompressY = stream.ReadFloat();
                            float decompressZ = stream.ReadFloat();

                            int playerCount = stream.ReadInt8();
                            while (playerCount > 0)
                            {
                                playerCount--;
                                ulong steamID = stream.ReadUInt64();

                                //TODO, can compressed further later.
                                ushort com_posX = stream.ReadUInt16();
                                ushort com_posY = stream.ReadUInt16();
                                ushort com_posZ = stream.ReadUInt16();
                                byte com_healt = stream.ReadInt8();
                                PlayerStand standing = (PlayerStand)stream.ReadInt8();
                                LeaningSide side = (LeaningSide)stream.ReadInt8();
                                LoadoutIndex loadoutIndex = (LoadoutIndex)stream.ReadInt8();
                                bool inSeat = stream.ReadBool();
                                bool isBleeding = stream.ReadBool();
                                ushort ping = stream.ReadUInt16();

                                var @internal = mInstanceDatabase.GetPlayerInternals(steamID);
                                if (@internal.IsAlive)
                                {
                                    @internal.Position = new Vector3()
                                    {
                                        X = com_posX * decompressX,
                                        Y = com_posY * decompressY,
                                        Z = com_posZ * decompressZ,
                                    };
                                    @internal.HP = (com_healt * 0.5f) - 1f;
                                    @internal.Standing = standing;
                                    @internal.Leaning = side;
                                    @internal.CurrentLoadoutIndex = loadoutIndex;
                                    @internal.InVehicle = inSeat;
                                    @internal.IsBleeding = isBleeding;
                                    @internal.PingMs = ping;
                                }
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerGivenUp:
                    {
                        if (stream.CanRead(8))
                        {
                            ulong steamID = stream.ReadUInt64();
                            if (resources.TryGetPlayer(steamID, out var client))
                            {
                                client.OnGivenUp();
                                server.OnPlayerGivenUp((TPlayer)client);
                            }
                        }
                        break;
                    }
                case NetworkCommuncation.OnPlayerRevivedAnother:
                    {
                        if (stream.CanRead(8 + 8))
                        {
                            ulong from = stream.ReadUInt64();
                            ulong to = stream.ReadUInt64();
                            if (resources.TryGetPlayer(to, out var toClient))
                            {
                                toClient.OnRevivedByAnotherPlayer();

                                if (resources.TryGetPlayer(from, out var fromClient))
                                {
                                    fromClient.OnRevivedAnotherPlayer();
                                    server.OnAPlayerRevivedAnotherPlayer((TPlayer)fromClient, (TPlayer)toClient);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        // --- Public ---
        public IEnumerable<TGameServer> ConnectedGameServers
        {
            get
            {
                var list = new List<TGameServer>(mActiveConnections.Count);
                lock (mActiveConnections)
                {
                    foreach (var item in mActiveConnections.Values)
                        list.Add(item.server);
                }
                return list;
            }
        }
        public bool TryGetGameServer(IPAddress ip, ushort port, out TGameServer server)
        {
            var hash = ((ulong)port << 32) | (ulong)ip.ToUInt();
            lock (mActiveConnections)
            {
                if (mActiveConnections.TryGetValue(hash, out var _server))
                {
                    server = (TGameServer)_server.server;
                    return true;
                }
            }

            server = default;
            return false;
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

        // --- Classes --- 
        private class mInstances<TPlayer, TGameServer> where TPlayer : Player<TPlayer> where TGameServer : GameServer<TPlayer>
        {
            private Dictionary<ulong, (TGameServer, GameServer<TPlayer>.Internal)> mGameServerInstances;
            private Dictionary<ulong, (TPlayer, Player<TPlayer>.Internal)> mPlayerInstances;

            public mInstances()
            {
                this.mGameServerInstances = new Dictionary<ulong, (TGameServer, GameServer<TPlayer>.Internal)>(64);
                this.mPlayerInstances = new Dictionary<ulong, (TPlayer, Player<TPlayer>.Internal)>(1024 * 16);
            }

            public TGameServer GetServerInstance(ulong hash, out bool isNew, out GameServer<TPlayer>.Internal @internal)
            {
                lock (mGameServerInstances)
                {
                    if (mGameServerInstances.TryGetValue(hash, out var data))
                    {
                        @internal = data.Item2;
                        isNew = false;
                        return data.Item1;
                    }

                    @internal = new GameServer<TPlayer>.Internal();
                    TGameServer gameServer = GameServer<TPlayer>.CreateInstance<TGameServer>(@internal);

                    isNew = true;
                    mGameServerInstances.Add(hash, (gameServer, @internal));
                    return gameServer;
                }
            }
            public TPlayer GetPlayerInstance(ulong steamID, out bool isNew, out Player<TPlayer>.Internal @internal)
            {
                lock (this.mPlayerInstances)
                {
                    if (this.mPlayerInstances.TryGetValue(steamID, out var player))
                    {
                        isNew = false;
                        @internal = player.Item2;
                        return player.Item1;
                    }

                    @internal = new Player<TPlayer>.Internal();
                    var pplayer = Player<TPlayer>.CreateInstance(@internal);

                    isNew = true;
                    mPlayerInstances.Add(steamID, (pplayer, @internal));
                    return pplayer;
                }
            }
            public Player<TPlayer>.Internal GetPlayerInternals(ulong steamID)
            {
                lock (mPlayerInstances)
                    return mPlayerInstances[steamID].Item2;
            }
        }
    }
}
