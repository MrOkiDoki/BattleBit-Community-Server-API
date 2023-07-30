namespace BattleBitAPI.Common
{
    public class PlayerStats
    {
        public PlayerStats() { }
        public PlayerStats(byte[] data) { Load(data); }

        public bool IsBanned;
        public Roles Roles;
        public PlayerProgess Progress = new PlayerProgess();
        public byte[] ToolProgress;
        public byte[] Achievements;
        public byte[] Selections;

        public void Write(Common.Serialization.Stream ser)
        {
            ser.Write(this.IsBanned);
            ser.Write((ulong)this.Roles);

            Progress.Write(ser);

            if (ToolProgress != null)
            {
                ser.Write((ushort)ToolProgress.Length);
                ser.Write(ToolProgress, 0, ToolProgress.Length);
            }
            else
            {
                ser.Write((ushort)0);
            }

            if (Achievements != null)
            {
                ser.Write((ushort)Achievements.Length);
                ser.Write(Achievements, 0, Achievements.Length);
            }
            else
            {
                ser.Write((ushort)0);
            }

            if (Selections != null)
            {
                ser.Write((ushort)Selections.Length);
                ser.Write(Selections, 0, Selections.Length);
            }
            else
            {
                ser.Write((ushort)0);
            }
        }
        public void Read(Common.Serialization.Stream ser)
        {
            this.IsBanned = ser.ReadBool();
            this.Roles = (Roles)ser.ReadUInt64();

            this.Progress.Read(ser);

            int size = ser.ReadInt16();
            this.ToolProgress = ser.ReadByteArray(size);

            size = ser.ReadInt16();
            this.Achievements = ser.ReadByteArray(size);

            size = ser.ReadInt16();
            this.Selections = ser.ReadByteArray(size);
        }

        public byte[] SerializeToByteArray()
        {
            using (var ser = Common.Serialization.Stream.Get())
            {
                Write(ser);
                return ser.AsByteArrayData();
            }
        }
        public void Load(byte[] data)
        {
            var ser = new Common.Serialization.Stream()
            {
                Buffer = data,
                InPool = false,
                ReadPosition = 0,
                WritePosition = data.Length,
            };
            Read(ser);
        }

        public class PlayerProgess
        {
            private const uint ParamCount = 42;

            public uint KillCount;
            public uint LeaderKills;
            public uint AssaultKills;
            public uint MedicKills;
            public uint EngineerKills;
            public uint SupportKills;
            public uint ReconKills;
            public uint DeathCount;
            public uint WinCount;
            public uint LoseCount;
            public uint FriendlyShots;
            public uint FriendlyKills;
            public uint Revived;
            public uint RevivedTeamMates;
            public uint Assists;
            public uint Prestige;
            public uint Rank;
            public uint EXP;
            public uint ShotsFired;
            public uint ShotsHit;
            public uint Headshots;
            public uint ObjectivesComplated;
            public uint HealedHPs;
            public uint RoadKills;
            public uint Suicides;
            public uint VehiclesDestroyed;
            public uint VehicleHPRepaired;
            public uint LongestKill;
            public uint PlayTimeSeconds;
            public uint LeaderPlayTime;
            public uint AssaultPlayTime;
            public uint MedicPlayTime;
            public uint EngineerPlayTime;
            public uint SupportPlayTime;
            public uint ReconPlayTime;
            public uint LeaderScore;
            public uint AssaultScore;
            public uint MedicScore;
            public uint EngineerScore;
            public uint SupportScore;
            public uint ReconScore;
            public uint TotalScore;

            public void Write(Common.Serialization.Stream ser)
            {
                ser.Write(ParamCount);
                {
                    ser.Write(KillCount);
                    ser.Write(LeaderKills);
                    ser.Write(AssaultKills);
                    ser.Write(MedicKills);
                    ser.Write(EngineerKills);
                    ser.Write(SupportKills);
                    ser.Write(ReconKills);
                    ser.Write(DeathCount);
                    ser.Write(WinCount);
                    ser.Write(LoseCount);
                    ser.Write(FriendlyShots);
                    ser.Write(FriendlyKills);
                    ser.Write(Revived);
                    ser.Write(RevivedTeamMates);
                    ser.Write(Assists);
                    ser.Write(Prestige);
                    ser.Write(Rank);
                    ser.Write(EXP);
                    ser.Write(ShotsFired);
                    ser.Write(ShotsHit);
                    ser.Write(Headshots);
                    ser.Write(ObjectivesComplated);
                    ser.Write(HealedHPs);
                    ser.Write(RoadKills);
                    ser.Write(Suicides);
                    ser.Write(VehiclesDestroyed);
                    ser.Write(VehicleHPRepaired);
                    ser.Write(LongestKill);
                    ser.Write(PlayTimeSeconds);
                    ser.Write(LeaderPlayTime);
                    ser.Write(AssaultPlayTime);
                    ser.Write(MedicPlayTime);
                    ser.Write(EngineerPlayTime);
                    ser.Write(SupportPlayTime);
                    ser.Write(ReconPlayTime);
                    ser.Write(LeaderScore);
                    ser.Write(AssaultScore);
                    ser.Write(MedicScore);
                    ser.Write(EngineerScore);
                    ser.Write(SupportScore);
                    ser.Write(ReconScore);
                    ser.Write(TotalScore);
                }
            }
            public void Read(Common.Serialization.Stream ser)
            {
                Reset();

                uint mParamCount = ser.ReadUInt32();
                int maxReadPosition = ser.ReadPosition + (int)(mParamCount * 4);
                {
                    bool canRead() => ser.ReadPosition < maxReadPosition;
                    if (canRead())
                        this.KillCount = ser.ReadUInt32();
                    if (canRead())
                        this.LeaderKills = ser.ReadUInt32();
                    if (canRead())
                        this.AssaultKills = ser.ReadUInt32();
                    if (canRead())
                        this.MedicKills = ser.ReadUInt32();
                    if (canRead())
                        this.EngineerKills = ser.ReadUInt32();
                    if (canRead())
                        this.SupportKills = ser.ReadUInt32();
                    if (canRead())
                        this.ReconKills = ser.ReadUInt32();
                    if (canRead())
                        this.DeathCount = ser.ReadUInt32();
                    if (canRead())
                        this.WinCount = ser.ReadUInt32();
                    if (canRead())
                        this.LoseCount = ser.ReadUInt32();
                    if (canRead())
                        this.FriendlyShots = ser.ReadUInt32();
                    if (canRead())
                        this.FriendlyKills = ser.ReadUInt32();
                    if (canRead())
                        this.Revived = ser.ReadUInt32();
                    if (canRead())
                        this.RevivedTeamMates = ser.ReadUInt32();
                    if (canRead())
                        this.Assists = ser.ReadUInt32();
                    if (canRead())
                        this.Prestige = ser.ReadUInt32();
                    if (canRead())
                        this.Rank = ser.ReadUInt32();
                    if (canRead())
                        this.EXP = ser.ReadUInt32();
                    if (canRead())
                        this.ShotsFired = ser.ReadUInt32();
                    if (canRead())
                        this.ShotsHit = ser.ReadUInt32();
                    if (canRead())
                        this.Headshots = ser.ReadUInt32();
                    if (canRead())
                        this.ObjectivesComplated = ser.ReadUInt32();
                    if (canRead())
                        this.HealedHPs = ser.ReadUInt32();
                    if (canRead())
                        this.RoadKills = ser.ReadUInt32();
                    if (canRead())
                        this.Suicides = ser.ReadUInt32();
                    if (canRead())
                        this.VehiclesDestroyed = ser.ReadUInt32();
                    if (canRead())
                        this.VehicleHPRepaired = ser.ReadUInt32();
                    if (canRead())
                        this.LongestKill = ser.ReadUInt32();
                    if (canRead())
                        this.PlayTimeSeconds = ser.ReadUInt32();
                    if (canRead())
                        this.LeaderPlayTime = ser.ReadUInt32();
                    if (canRead())
                        this.AssaultPlayTime = ser.ReadUInt32();
                    if (canRead())
                        this.MedicPlayTime = ser.ReadUInt32();
                    if (canRead())
                        this.EngineerPlayTime = ser.ReadUInt32();
                    if (canRead())
                        this.SupportPlayTime = ser.ReadUInt32();
                    if (canRead())
                        this.ReconPlayTime = ser.ReadUInt32();
                    if (canRead())
                        this.LeaderScore = ser.ReadUInt32();
                    if (canRead())
                        this.AssaultScore = ser.ReadUInt32();
                    if (canRead())
                        this.MedicScore = ser.ReadUInt32();
                    if (canRead())
                        this.EngineerScore = ser.ReadUInt32();
                    if (canRead())
                        this.SupportScore = ser.ReadUInt32();
                    if (canRead())
                        this.ReconScore = ser.ReadUInt32();
                    if (canRead())
                        this.TotalScore = ser.ReadUInt32();
                }
                ser.ReadPosition = maxReadPosition;
            }
            public void Reset()
            {
                KillCount = 0;
                LeaderKills = 0;
                AssaultKills = 0;
                MedicKills = 0;
                EngineerKills = 0;
                SupportKills = 0;
                ReconKills = 0;
                DeathCount = 0;
                WinCount = 0;
                LoseCount = 0;
                FriendlyShots = 0;
                FriendlyKills = 0;
                Revived = 0;
                RevivedTeamMates = 0;
                Assists = 0;
                Prestige = 0;
                Rank = 0;
                EXP = 0;
                ShotsFired = 0;
                ShotsHit = 0;
                Headshots = 0;
                ObjectivesComplated = 0;
                HealedHPs = 0;
                RoadKills = 0;
                Suicides = 0;
                VehiclesDestroyed = 0;
                VehicleHPRepaired = 0;
                LongestKill = 0;
                PlayTimeSeconds = 0;
                LeaderPlayTime = 0;
                AssaultPlayTime = 0;
                MedicPlayTime = 0;
                EngineerPlayTime = 0;
                SupportPlayTime = 0;
                ReconPlayTime = 0;
                LeaderScore = 0;
                AssaultScore = 0;
                MedicScore = 0;
                EngineerScore = 0;
                SupportScore = 0;
                ReconScore = 0;
                TotalScore = 0;
            }
        }
    }
}
