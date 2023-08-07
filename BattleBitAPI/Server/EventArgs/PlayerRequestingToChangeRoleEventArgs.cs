using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerRequestingToChangeRoleEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player requesting.
        /// </summary>
        public TPlayer Player { get; init; }

        /// <summary>
        /// The role the player asking to change.
        /// </summary>
        public GameRole Role { get; init; }

        /// <summary>
        /// Whether to allow the player to change role or not.
        /// </summary>
        public bool Allow { get; set; }

        internal PlayerRequestingToChangeRoleEventArgs(TPlayer player, GameRole role)
        {
            Player = player;
            Role = role;
            Allow = true;
        }
    }
}