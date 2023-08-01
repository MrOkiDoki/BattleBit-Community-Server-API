namespace BattleBitAPI.Server
{
    public class ServerSettings
    {
        private GameServer.mInternalResources mResources;
        public ServerSettings(GameServer.mInternalResources resources)
        {
            mResources = resources;
        }

        public float DamageMultiplier
        {
            get => mResources.Settings.DamageMultiplier;
            set
            {
                mResources.Settings.DamageMultiplier = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool BleedingEnabled
        {
            get => mResources.Settings.BleedingEnabled;
            set
            {
                mResources.Settings.BleedingEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool StamineEnabled
        {
            get => mResources.Settings.StamineEnabled;
            set
            {
                mResources.Settings.StamineEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool FriendlyFireEnabled
        {
            get => mResources.Settings.FriendlyFireEnabled;
            set
            {
                mResources.Settings.FriendlyFireEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool OnlyWinnerTeamCanVote
        {
            get => mResources.Settings.OnlyWinnerTeamCanVote;
            set
            {
                mResources.Settings.OnlyWinnerTeamCanVote = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool HitMarkersEnabled
        {
            get => mResources.Settings.HitMarkersEnabled;
            set
            {
                mResources.Settings.HitMarkersEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool PointLogEnabled
        {
            get => mResources.Settings.PointLogEnabled;
            set
            {
                mResources.Settings.PointLogEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool SpectatorEnabled
        {
            get => mResources.Settings.SpectatorEnabled;
            set
            {
                mResources.Settings.SpectatorEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
    }
}
