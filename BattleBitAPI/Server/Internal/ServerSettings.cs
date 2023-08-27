using System.Runtime.ConstrainedExecution;

namespace BattleBitAPI.Server
{
    public class ServerSettings<TPlayer> where TPlayer : Player<TPlayer>
    {
        // ---- Construction ---- 
        private GameServer<TPlayer>.Internal mResources;
        public ServerSettings(GameServer<TPlayer>.Internal resources)
        {
            mResources = resources;
        }

        // ---- Variables ---- 
        public float DamageMultiplier
        {
            get => mResources._RoomSettings.DamageMultiplier;
            set
            {
                if (mResources._RoomSettings.DamageMultiplier == value)
                    return;
                mResources._RoomSettings.DamageMultiplier = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool FriendlyFireEnabled
        {
            get => mResources._RoomSettings.FriendlyFireEnabled;
            set
            {
                if (mResources._RoomSettings.FriendlyFireEnabled == value)
                    return;
                mResources._RoomSettings.FriendlyFireEnabled = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool OnlyWinnerTeamCanVote
        {
            get => mResources._RoomSettings.OnlyWinnerTeamCanVote;
            set
            {
                if (mResources._RoomSettings.OnlyWinnerTeamCanVote == value)
                    return;
                mResources._RoomSettings.OnlyWinnerTeamCanVote = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool PlayerCollision
        {
            get => mResources._RoomSettings.PlayerCollision;
            set
            {
                if (mResources._RoomSettings.PlayerCollision == value)
                    return;
                mResources._RoomSettings.PlayerCollision = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool HideMapVotes
        {
            get => mResources._RoomSettings.HideMapVotes;
            set
            {
                if (mResources._RoomSettings.HideMapVotes == value)
                    return;
                mResources._RoomSettings.HideMapVotes = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool CanVoteDay
        {
            get => mResources._RoomSettings.CanVoteDay;
            set
            {
                if (mResources._RoomSettings.CanVoteDay == value)
                    return;
                mResources._RoomSettings.CanVoteDay = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool CanVoteNight
        {
            get => mResources._RoomSettings.CanVoteNight;
            set
            {
                if (mResources._RoomSettings.CanVoteNight == value)
                    return;
                mResources._RoomSettings.CanVoteNight = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }

        public byte MedicLimitPerSquad
        {
            get => mResources._RoomSettings.MedicLimitPerSquad;
            set
            {
                if (mResources._RoomSettings.MedicLimitPerSquad == value)
                    return;
                mResources._RoomSettings.MedicLimitPerSquad = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public byte EngineerLimitPerSquad
        {
            get => mResources._RoomSettings.EngineerLimitPerSquad;
            set
            {
                if (mResources._RoomSettings.EngineerLimitPerSquad == value)
                    return;
                mResources._RoomSettings.EngineerLimitPerSquad = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public byte SupportLimitPerSquad
        {
            get => mResources._RoomSettings.SupportLimitPerSquad;
            set
            {
                if (mResources._RoomSettings.SupportLimitPerSquad == value)
                    return;
                mResources._RoomSettings.SupportLimitPerSquad = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public byte ReconLimitPerSquad
        {
            get => mResources._RoomSettings.ReconLimitPerSquad;
            set
            {
                if (mResources._RoomSettings.ReconLimitPerSquad == value)
                    return;
                mResources._RoomSettings.ReconLimitPerSquad = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }

        public float TankSpawnDelayMultipler
        {
            get => mResources._RoomSettings.TankSpawnDelayMultipler;
            set
            {
                if (mResources._RoomSettings.TankSpawnDelayMultipler == value)
                    return;
                mResources._RoomSettings.TankSpawnDelayMultipler = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public float TransportSpawnDelayMultipler
        {
            get => mResources._RoomSettings.TransportSpawnDelayMultipler;
            set
            {
                if (mResources._RoomSettings.TransportSpawnDelayMultipler == value)
                    return;
                mResources._RoomSettings.TransportSpawnDelayMultipler = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public float SeaVehicleSpawnDelayMultipler
        {
            get => mResources._RoomSettings.SeaVehicleSpawnDelayMultipler;
            set
            {
                if (mResources._RoomSettings.SeaVehicleSpawnDelayMultipler == value)
                    return;
                mResources._RoomSettings.SeaVehicleSpawnDelayMultipler = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public float APCSpawnDelayMultipler
        {
            get => mResources._RoomSettings.APCSpawnDelayMultipler;
            set
            {
                if (mResources._RoomSettings.APCSpawnDelayMultipler == value)
                    return;
                mResources._RoomSettings.APCSpawnDelayMultipler = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public float HelicopterSpawnDelayMultipler
        {
            get => mResources._RoomSettings.HelicopterSpawnDelayMultipler;
            set
            {
                if (mResources._RoomSettings.HelicopterSpawnDelayMultipler == value)
                    return;
                mResources._RoomSettings.HelicopterSpawnDelayMultipler = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }

        public bool UnlockAllAttachments
        {
            get => mResources._RoomSettings.UnlockAllAttachments;
            set
            {
                if (mResources._RoomSettings.UnlockAllAttachments == value)
                    return;
                mResources._RoomSettings.UnlockAllAttachments = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }

        // ---- Reset ---- 
        public void Reset()
        {

        }

        // ---- Classes ---- 
        public class mRoomSettings
        {
            public float DamageMultiplier = 1.0f;
            public bool FriendlyFireEnabled = false;
            public bool HideMapVotes = true;
            public bool OnlyWinnerTeamCanVote = false;
            public bool PlayerCollision = false;

            public byte MedicLimitPerSquad = 8;
            public byte EngineerLimitPerSquad = 8;
            public byte SupportLimitPerSquad = 8;
            public byte ReconLimitPerSquad = 8;

            public bool CanVoteDay = true;
            public bool CanVoteNight = true;

            public float TankSpawnDelayMultipler = 1.0f;
            public float TransportSpawnDelayMultipler = 1.0f;
            public float SeaVehicleSpawnDelayMultipler = 1.0f;
            public float APCSpawnDelayMultipler = 1.0f;
            public float HelicopterSpawnDelayMultipler = 1.0f;

            public bool UnlockAllAttachments = false;

            public void Write(Common.Serialization.Stream ser)
            {
                ser.Write(this.DamageMultiplier);
                ser.Write(this.FriendlyFireEnabled);
                ser.Write(this.HideMapVotes);
                ser.Write(this.OnlyWinnerTeamCanVote);
                ser.Write(this.PlayerCollision);

                ser.Write(this.MedicLimitPerSquad);
                ser.Write(this.EngineerLimitPerSquad);
                ser.Write(this.SupportLimitPerSquad);
                ser.Write(this.ReconLimitPerSquad);

                ser.Write(this.CanVoteDay);
                ser.Write(this.CanVoteNight);

                ser.Write(this.TankSpawnDelayMultipler);
                ser.Write(this.TransportSpawnDelayMultipler);
                ser.Write(this.SeaVehicleSpawnDelayMultipler);
                ser.Write(this.APCSpawnDelayMultipler);
                ser.Write(this.HelicopterSpawnDelayMultipler);

                ser.Write(this.UnlockAllAttachments);
            }
            public void Read(Common.Serialization.Stream ser)
            {
                this.DamageMultiplier = ser.ReadFloat();
                this.FriendlyFireEnabled = ser.ReadBool();
                this.HideMapVotes = ser.ReadBool();
                this.OnlyWinnerTeamCanVote = ser.ReadBool();
                this.PlayerCollision=ser.ReadBool();

                this.MedicLimitPerSquad = ser.ReadInt8();
                this.EngineerLimitPerSquad = ser.ReadInt8();
                this.SupportLimitPerSquad = ser.ReadInt8();
                this.ReconLimitPerSquad = ser.ReadInt8();

                this.CanVoteDay = ser.ReadBool();
                this.CanVoteNight = ser.ReadBool();

                this.TankSpawnDelayMultipler = ser.ReadFloat();
                this.TransportSpawnDelayMultipler = ser.ReadFloat();
                this.SeaVehicleSpawnDelayMultipler = ser.ReadFloat();
                this.APCSpawnDelayMultipler = ser.ReadFloat();
                this.HelicopterSpawnDelayMultipler = ser.ReadFloat();

                this.UnlockAllAttachments = ser.ReadBool();
            }
            public void Reset()
            {
                this.DamageMultiplier = 1.0f;
                this.FriendlyFireEnabled = false;
                this.HideMapVotes = true;
                this.OnlyWinnerTeamCanVote = false;
                this.PlayerCollision = false;

                this.MedicLimitPerSquad = 8;
                this.EngineerLimitPerSquad = 8;
                this.SupportLimitPerSquad = 8;
                this.ReconLimitPerSquad = 8;

                this.CanVoteDay = true;
                this.CanVoteNight = true;

                this.TankSpawnDelayMultipler = 1.0f;
                this.TransportSpawnDelayMultipler = 1.0f;
                this.SeaVehicleSpawnDelayMultipler = 1.0f;
                this.APCSpawnDelayMultipler = 1.0f;
                this.HelicopterSpawnDelayMultipler = 1.0f;

                this.UnlockAllAttachments = false;
            }
        }
    }
}
