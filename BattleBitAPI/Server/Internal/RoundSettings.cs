using BattleBitAPI.Common;

namespace BattleBitAPI.Server
{
    public class RoundSettings<TPlayer> where TPlayer : Player<TPlayer>
    {
        private GameServer<TPlayer>.Internal mResources;
        public RoundSettings(GameServer<TPlayer>.Internal resources)
        {
            mResources = resources;
        }

        public GameState State
        {
            get => this.mResources._RoundSettings.State;
        }
        public double TeamATickets
        {
            get => this.mResources._RoundSettings.TeamATickets;
            set
            {
                this.mResources._RoundSettings.TeamATickets = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }
        public double TeamBTickets
        {
            get => this.mResources._RoundSettings.TeamBTickets;
            set
            {
                this.mResources._RoundSettings.TeamBTickets = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }
        public double MaxTickets
        {
            get => this.mResources._RoundSettings.MaxTickets;
            set
            {
                this.mResources._RoundSettings.MaxTickets = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }
        public int PlayersToStart
        {
            get => this.mResources._RoundSettings.PlayersToStart;
            set
            {
                this.mResources._RoundSettings.PlayersToStart = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }
        public int SecondsLeft
        {
            get => this.mResources._RoundSettings.SecondsLeft;
            set
            {
                this.mResources._RoundSettings.SecondsLeft = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }

        public void Reset()
        {

        }
    }
}
