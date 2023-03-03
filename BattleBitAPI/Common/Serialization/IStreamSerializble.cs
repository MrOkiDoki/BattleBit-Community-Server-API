namespace BattleBitAPI.Common.Serialization
{
    public interface IStreamSerializable
    {
        void Read(Stream ser);
        void Write(Stream ser);
    }
}
