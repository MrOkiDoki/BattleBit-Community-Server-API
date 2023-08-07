using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerLeftSquadEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player who left the squad.
        /// </summary>
        public TPlayer Player { get; }

        /// <summary>
        /// The squad the player left.
        /// </summary>
        public Squads Squads { get; }

        internal PlayerLeftSquadEventArgs(TPlayer player, Squads squad)
        {
            Player = player;
            Squads = squad;
        }
    }
}