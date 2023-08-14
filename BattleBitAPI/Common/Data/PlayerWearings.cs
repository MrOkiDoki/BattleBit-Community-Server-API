using Stream = BattleBitAPI.Common.Serialization.Stream;

namespace BattleBitAPI.Common;

public struct PlayerWearings
{
    public string Head;
    public string Chest;
    public string Belt;
    public string Backbag;
    public string Eye;
    public string Face;
    public string Hair;
    public string Skin;
    public string Uniform;
    public string Camo;

    public void Write(Stream ser)
    {
        ser.WriteStringItem(Head);
        ser.WriteStringItem(Chest);
        ser.WriteStringItem(Belt);
        ser.WriteStringItem(Backbag);
        ser.WriteStringItem(Eye);
        ser.WriteStringItem(Face);
        ser.WriteStringItem(Hair);
        ser.WriteStringItem(Skin);
        ser.WriteStringItem(Uniform);
        ser.WriteStringItem(Camo);
    }

    public void Read(Stream ser)
    {
        ser.TryReadString(out Head);
        ser.TryReadString(out Chest);
        ser.TryReadString(out Belt);
        ser.TryReadString(out Backbag);
        ser.TryReadString(out Eye);
        ser.TryReadString(out Face);
        ser.TryReadString(out Hair);
        ser.TryReadString(out Skin);
        ser.TryReadString(out Uniform);
        ser.TryReadString(out Camo);
    }
}