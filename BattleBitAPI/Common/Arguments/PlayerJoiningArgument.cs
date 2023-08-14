using System;

namespace BattleBitAPI.Common
{
    public class PlayerJoiningArguments
    {
        public PlayerStats Stats;
        public Team Team;
        public Squads Squad;

        public void Write(BattleBitAPI.Common.Serialization.Stream ser)
        {
            this.Stats.Write(ser);
            ser.Write((byte)this.Team);
            ser.Write((byte)this.Squad);
        }
    }
}
