namespace BattleBitAPI.Common
{
    public struct VoxelBlockData
    {
        public VoxelTextures TextureID;

        public void Write(BattleBitAPI.Common.Serialization.Stream ser)
        {
            ser.Write((byte)TextureID);
        }
        public void Read(BattleBitAPI.Common.Serialization.Stream ser)
        {
            this.TextureID = (VoxelTextures)ser.ReadInt8();
        }
    }
}
