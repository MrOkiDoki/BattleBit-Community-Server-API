using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerJoinedSquadEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player who joined the squad.
        /// </summary>
        public TPlayer Player { get; }

        /// <summary>
        /// The squad the player joined.
        /// </summary>
        public Squads Squads { get; }

        internal PlayerJoinedSquadEventArgs(TPlayer player, Squads squad)
        {
            Player = player;
            Squads = squad;
        }
    }
}