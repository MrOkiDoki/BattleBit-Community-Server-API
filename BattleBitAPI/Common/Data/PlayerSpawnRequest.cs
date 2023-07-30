using System.Numerics;

namespace BattleBitAPI.Common
{
    public struct PlayerSpawnRequest
    {
        public PlayerSpawningPosition RequestedPoint;
        public PlayerLoadout Loadout;
        public PlayerWearings Wearings;
        public Vector3 SpawnPosition;
        public Vector3 LookDirection;
        public PlayerStand SpawnStand;
        public float SpawnProtection;

        public void Write(Common.Serialization.Stream ser)
        {
            ser.Write((byte)this.RequestedPoint);
            this.Loadout.Write(ser);
            this.Wearings.Write(ser);
            ser.Write(this.SpawnPosition.X);
            ser.Write(this.SpawnPosition.Y);
            ser.Write(this.SpawnPosition.Z);
            ser.Write(this.LookDirection.X);
            ser.Write(this.LookDirection.Y);
            ser.Write(this.LookDirection.Z);
            ser.Write((byte)this.SpawnStand);
            ser.Write(this.SpawnProtection);
        }
        public void Read(Common.Serialization.Stream ser)
        {
            this.RequestedPoint = (PlayerSpawningPosition)ser.ReadInt8();
            this.Loadout.Read(ser);
            this.Wearings.Read(ser);
            this.SpawnPosition = new Vector3()
            {
                X = ser.ReadFloat(),
                Y = ser.ReadFloat(),
                Z = ser.ReadFloat()
            };
            this.LookDirection = new Vector3()
            {
                X = ser.ReadFloat(),
                Y = ser.ReadFloat(),
                Z = ser.ReadFloat()
            };
            this.SpawnStand = (PlayerStand)ser.ReadInt8();
            this.SpawnProtection = ser.ReadFloat();
        }
    }
}
