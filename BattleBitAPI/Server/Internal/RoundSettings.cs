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
        public int TeamATickets
        {
            get => this.mResources._RoundSettings.TeamATickets;
            set
            {
                this.mResources._RoundSettings.TeamATickets = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }
        public int TeamAMaxTickets
        {
            get => this.mResources._RoundSettings.TeamAMaxTickets;
            set
            {
                this.mResources._RoundSettings.TeamAMaxTickets = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }
        public int TeamBTickets
        {
            get => this.mResources._RoundSettings.TeamBTickets;
            set
            {
                this.mResources._RoundSettings.TeamBTickets = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }
        public int TeamBMaxTickets
        {
            get => this.mResources._RoundSettings.TeamBMaxTickets;
            set
            {
                this.mResources._RoundSettings.TeamBTickets = value;
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
        public int SecondsLeftToEndOfRound
        {
            get => this.mResources._RoundSettings.SecondsLeftToEndOfRound;
            set
            {
                this.mResources._RoundSettings.SecondsLeftToEndOfRound = value;
                this.mResources.IsDirtyRoundSettings = true;
            }
        }

        public void Reset()
        {

        }
    }
}
