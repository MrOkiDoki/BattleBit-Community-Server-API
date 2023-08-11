using System.Numerics;

namespace BattleBitAPI.Common
{
    public struct OnPlayerSpawnArguments
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
            ser.Write((byte)RequestedPoint);
            Loadout.Write(ser);
            Wearings.Write(ser);
            ser.Write(SpawnPosition.X);
            ser.Write(SpawnPosition.Y);
            ser.Write(SpawnPosition.Z);
            ser.Write(LookDirection.X);
            ser.Write(LookDirection.Y);
            ser.Write(LookDirection.Z);
            ser.Write((byte)SpawnStand);
            ser.Write(SpawnProtection);
        }
        public void Read(Common.Serialization.Stream ser)
        {
            RequestedPoint = (PlayerSpawningPosition)ser.ReadInt8();
            Loadout.Read(ser);
            Wearings.Read(ser);
            SpawnPosition = new Vector3()
            {
                X = ser.ReadFloat(),
                Y = ser.ReadFloat(),
                Z = ser.ReadFloat()
            };
            LookDirection = new Vector3()
            {
                X = ser.ReadFloat(),
                Y = ser.ReadFloat(),
                Z = ser.ReadFloat()
            };
            SpawnStand = (PlayerStand)ser.ReadInt8();
            SpawnProtection = ser.ReadFloat();
        }
    }
}
