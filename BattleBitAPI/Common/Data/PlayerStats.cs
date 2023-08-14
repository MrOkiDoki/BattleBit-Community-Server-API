using Stream = BattleBitAPI.Common.Serialization.Stream;

namespace BattleBitAPI.Common;

public class PlayerStats
{
    public byte[] Achievements;

    public bool IsBanned;
    public PlayerProgess Progress = new();
    public Roles Roles;
    public byte[] Selections;
    public byte[] ToolProgress;

    public PlayerStats()
    {
    }

    public PlayerStats(byte[] data)
    {
        Load(data);
    }

    public void Write(Stream ser)
    {
        ser.Write(IsBanned);
        ser.Write((ulong)Roles);

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

    public void Read(Stream ser)
    {
        IsBanned = ser.ReadBool();
        Roles = (Roles)ser.ReadUInt64();

        Progress.Read(ser);

        int size = ser.ReadInt16();
        ToolProgress = ser.ReadByteArray(size);

        size = ser.ReadInt16();
        Achievements = ser.ReadByteArray(size);

        size = ser.ReadInt16();
        Selections = ser.ReadByteArray(size);
    }

    public byte[] SerializeToByteArray()
    {
        using (var ser = Stream.Get())
        {
            Write(ser);
            return ser.AsByteArrayData();
        }
    }

    public void Load(byte[] data)
    {
        var ser = new Stream
        {
            Buffer = data,
            InPool = false,
            ReadPosition = 0,
            WritePosition = data.Length
        };
        Read(ser);
    }

    public class PlayerProgess
    {
        private const uint ParamCount = 42;
        public uint AssaultKills;
        public uint AssaultPlayTime;
        public uint AssaultScore;
        public uint Assists;
        public uint DeathCount;
        public uint EngineerKills;
        public uint EngineerPlayTime;
        public uint EngineerScore;
        public uint EXP;
        public uint FriendlyKills;
        public uint FriendlyShots;
        public uint Headshots;
        public uint HealedHPs;

        public uint KillCount;
        public uint LeaderKills;
        public uint LeaderPlayTime;
        public uint LeaderScore;
        public uint LongestKill;
        public uint LoseCount;
        public uint MedicKills;
        public uint MedicPlayTime;
        public uint MedicScore;
        public uint ObjectivesComplated;
        public uint PlayTimeSeconds;
        public uint Prestige;
        public uint Rank;
        public uint ReconKills;
        public uint ReconPlayTime;
        public uint ReconScore;
        public uint Revived;
        public uint RevivedTeamMates;
        public uint RoadKills;
        public uint ShotsFired;
        public uint ShotsHit;
        public uint Suicides;
        public uint SupportKills;
        public uint SupportPlayTime;
        public uint SupportScore;
        public uint TotalScore;
        public uint VehicleHPRepaired;
        public uint VehiclesDestroyed;
        public uint WinCount;

        public void Write(Stream ser)
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

        public void Read(Stream ser)
        {
            Reset();

            var mParamCount = ser.ReadUInt32();
            var maxReadPosition = ser.ReadPosition + (int)(mParamCount * 4);
            {
                bool canRead()
                {
                    return ser.ReadPosition < maxReadPosition;
                }

                if (canRead())
                    KillCount = ser.ReadUInt32();
                if (canRead())
                    LeaderKills = ser.ReadUInt32();
                if (canRead())
                    AssaultKills = ser.ReadUInt32();
                if (canRead())
                    MedicKills = ser.ReadUInt32();
                if (canRead())
                    EngineerKills = ser.ReadUInt32();
                if (canRead())
                    SupportKills = ser.ReadUInt32();
                if (canRead())
                    ReconKills = ser.ReadUInt32();
                if (canRead())
                    DeathCount = ser.ReadUInt32();
                if (canRead())
                    WinCount = ser.ReadUInt32();
                if (canRead())
                    LoseCount = ser.ReadUInt32();
                if (canRead())
                    FriendlyShots = ser.ReadUInt32();
                if (canRead())
                    FriendlyKills = ser.ReadUInt32();
                if (canRead())
                    Revived = ser.ReadUInt32();
                if (canRead())
                    RevivedTeamMates = ser.ReadUInt32();
                if (canRead())
                    Assists = ser.ReadUInt32();
                if (canRead())
                    Prestige = ser.ReadUInt32();
                if (canRead())
                    Rank = ser.ReadUInt32();
                if (canRead())
                    EXP = ser.ReadUInt32();
                if (canRead())
                    ShotsFired = ser.ReadUInt32();
                if (canRead())
                    ShotsHit = ser.ReadUInt32();
                if (canRead())
                    Headshots = ser.ReadUInt32();
                if (canRead())
                    ObjectivesComplated = ser.ReadUInt32();
                if (canRead())
                    HealedHPs = ser.ReadUInt32();
                if (canRead())
                    RoadKills = ser.ReadUInt32();
                if (canRead())
                    Suicides = ser.ReadUInt32();
                if (canRead())
                    VehiclesDestroyed = ser.ReadUInt32();
                if (canRead())
                    VehicleHPRepaired = ser.ReadUInt32();
                if (canRead())
                    LongestKill = ser.ReadUInt32();
                if (canRead())
                    PlayTimeSeconds = ser.ReadUInt32();
                if (canRead())
                    LeaderPlayTime = ser.ReadUInt32();
                if (canRead())
                    AssaultPlayTime = ser.ReadUInt32();
                if (canRead())
                    MedicPlayTime = ser.ReadUInt32();
                if (canRead())
                    EngineerPlayTime = ser.ReadUInt32();
                if (canRead())
                    SupportPlayTime = ser.ReadUInt32();
                if (canRead())
                    ReconPlayTime = ser.ReadUInt32();
                if (canRead())
                    LeaderScore = ser.ReadUInt32();
                if (canRead())
                    AssaultScore = ser.ReadUInt32();
                if (canRead())
                    MedicScore = ser.ReadUInt32();
                if (canRead())
                    EngineerScore = ser.ReadUInt32();
                if (canRead())
                    SupportScore = ser.ReadUInt32();
                if (canRead())
                    ReconScore = ser.ReadUInt32();
                if (canRead())
                    TotalScore = ser.ReadUInt32();
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