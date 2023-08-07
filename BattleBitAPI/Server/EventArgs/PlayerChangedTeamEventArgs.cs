using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerChangedTeamEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player who joined a team.
        /// </summary>
        public TPlayer Player { get; init; }

        /// <summary>
        /// The new team which the player joined.
        /// </summary>
        public Team Team { get; init; }

        internal PlayerChangedTeamEventArgs(TPlayer player, Team team)
        {
            Player = player;
            Team = team;
        }
    }
}