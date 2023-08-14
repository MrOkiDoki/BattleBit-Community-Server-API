using Stream = BattleBitAPI.Common.Serialization.Stream;

namespace BattleBitAPI.Common;

public class PlayerJoiningArguments
{
    public Squads Squad;
    public PlayerStats Stats;
    public Team Team;

    public void Write(Stream ser)
    {
        Stats.Write(ser);
        ser.Write((byte)Team);
        ser.Write((byte)Squad);
    }
}