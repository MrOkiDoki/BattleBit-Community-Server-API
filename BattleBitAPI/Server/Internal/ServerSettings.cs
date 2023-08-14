using BattleBitAPI.Server;

namespace BattleBitAPI.Server
{
    public class ServerSettings<TPlayer> where TPlayer : Player<TPlayer>
    {
        private GameServer<TPlayer>.Internal mResources;
        public ServerSettings(GameServer<TPlayer>.Internal resources)
        {
            mResources = resources;
        }

        public float DamageMultiplier
        {
            get => mResources._RoomSettings.DamageMultiplier;
            set
            {
                mResources._RoomSettings.DamageMultiplier = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool BleedingEnabled
        {
            get => mResources._RoomSettings.BleedingEnabled;
            set
            {
                mResources._RoomSettings.BleedingEnabled = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool StamineEnabled
        {
            get => mResources._RoomSettings.StaminaEnabled;
            set
            {
                mResources._RoomSettings.StaminaEnabled = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool FriendlyFireEnabled
        {
            get => mResources._RoomSettings.FriendlyFireEnabled;
            set
            {
                mResources._RoomSettings.FriendlyFireEnabled = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool OnlyWinnerTeamCanVote
        {
            get => mResources._RoomSettings.OnlyWinnerTeamCanVote;
            set
            {
                mResources._RoomSettings.OnlyWinnerTeamCanVote = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool HitMarkersEnabled
        {
            get => mResources._RoomSettings.HitMarkersEnabled;
            set
            {
                mResources._RoomSettings.HitMarkersEnabled = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool PointLogEnabled
        {
            get => mResources._RoomSettings.PointLogEnabled;
            set
            {
                mResources._RoomSettings.PointLogEnabled = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }
        public bool SpectatorEnabled
        {
            get => mResources._RoomSettings.SpectatorEnabled;
            set
            {
                mResources._RoomSettings.SpectatorEnabled = value;
                mResources.IsDirtyRoomSettings = true;
            }
        }

        public void Reset()
        {

        }
    }
}
