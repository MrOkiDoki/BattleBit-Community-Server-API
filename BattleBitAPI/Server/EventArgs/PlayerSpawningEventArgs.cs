using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerSpawningEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player who is spawning.
        /// </summary>
        public TPlayer Player { get; init; }

        /// <summary>
        /// The spawn request (which you can modify).
        /// </summary>
        public PlayerSpawnRequest Request { get; set; }

        internal PlayerSpawningEventArgs(TPlayer player, PlayerSpawnRequest request)
        {
            Player = player;
            Request = request;
        }
    }
}