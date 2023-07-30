namespace BattleBitAPI.Common
{
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

        public void Write(Common.Serialization.Stream ser)
        {
            ser.WriteStringItem(this.Head);
            ser.WriteStringItem(this.Chest);
            ser.WriteStringItem(this.Belt);
            ser.WriteStringItem(this.Backbag);
            ser.WriteStringItem(this.Eye);
            ser.WriteStringItem(this.Face);
            ser.WriteStringItem(this.Hair);
            ser.WriteStringItem(this.Skin);
            ser.WriteStringItem(this.Uniform);
            ser.WriteStringItem(this.Camo);
        }
        public void Read(Common.Serialization.Stream ser)
        {
            ser.TryReadString(out this.Head);
            ser.TryReadString(out this.Chest);
            ser.TryReadString(out this.Belt);
            ser.TryReadString(out this.Backbag);
            ser.TryReadString(out this.Eye);
            ser.TryReadString(out this.Face);
            ser.TryReadString(out this.Hair);
            ser.TryReadString(out this.Skin);
            ser.TryReadString(out this.Uniform);
            ser.TryReadString(out this.Camo);
        }
    }
}
