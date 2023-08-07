using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerChangedRoleEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player who changed role.
        /// </summary>
        public TPlayer Player { get; init; }

        /// <summary>
        /// The new role of the player.
        /// </summary>
        public GameRole Role { get; init; }

        internal PlayerChangedRoleEventArgs(TPlayer player, GameRole role)
        {
            Player = player;
            Role = role;
        }
    }
}