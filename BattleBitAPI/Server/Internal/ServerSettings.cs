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
            get => mResources._Settings.DamageMultiplier;
            set
            {
                mResources._Settings.DamageMultiplier = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool BleedingEnabled
        {
            get => mResources._Settings.BleedingEnabled;
            set
            {
                mResources._Settings.BleedingEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool StamineEnabled
        {
            get => mResources._Settings.StamineEnabled;
            set
            {
                mResources._Settings.StamineEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool FriendlyFireEnabled
        {
            get => mResources._Settings.FriendlyFireEnabled;
            set
            {
                mResources._Settings.FriendlyFireEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool OnlyWinnerTeamCanVote
        {
            get => mResources._Settings.OnlyWinnerTeamCanVote;
            set
            {
                mResources._Settings.OnlyWinnerTeamCanVote = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool HitMarkersEnabled
        {
            get => mResources._Settings.HitMarkersEnabled;
            set
            {
                mResources._Settings.HitMarkersEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool PointLogEnabled
        {
            get => mResources._Settings.PointLogEnabled;
            set
            {
                mResources._Settings.PointLogEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }
        public bool SpectatorEnabled
        {
            get => mResources._Settings.SpectatorEnabled;
            set
            {
                mResources._Settings.SpectatorEnabled = value;
                mResources.IsDirtySettings = true;
            }
        }

        public void Reset()
        {

        }
    }
}
