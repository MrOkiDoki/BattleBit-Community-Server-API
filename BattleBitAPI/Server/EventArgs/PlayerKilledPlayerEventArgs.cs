using System.Numerics;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerKilledPlayerEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The killer.
        /// </summary>
        public TPlayer Killer { get; init; }
        
        /// <summary>
        /// The position of the killer.
        /// </summary>
        public Vector3 KillerPosition { get; init; }

        /// <summary>
        /// The target.
        /// </summary>
        public TPlayer Target { get; init; }

        /// <summary>
        /// The position of the target.
        /// </summary>
        public Vector3 TargetPosition { get; init; }

        /// <summary>
        /// The tool used to kill the target.
        /// </summary>
        public string Tool { get; init; }

        internal PlayerKilledPlayerEventArgs(TPlayer killer, Vector3 killerPosition, TPlayer target, Vector3 targetPosition, string tool)
        {
            Killer = killer;
            KillerPosition = killerPosition;
            Target = target;
            TargetPosition = targetPosition;
            Tool = tool;
        }
    }
}