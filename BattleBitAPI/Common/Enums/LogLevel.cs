namespace BattleBitAPI.Common
{
    [System.Flags]
    public enum LogLevel : ulong
    {
        None = 0,

        /// <summary>
        /// Output logs from low level sockets.
        /// </summary>
        Sockets = 1 << 0,

        /// <summary>
        /// Output logs from remote game server (Highly recommended)
        /// </summary>
        GameServerErrors = 1 << 1,
        
        /// <summary>
        /// Output logs of game server connects, reconnects.
        /// </summary>
        GameServers = 1 << 2,

        /// <summary>
        /// Output logs of player connects, disconnects
        /// </summary>
        Players = 1 << 3,

        /// <summary>
        /// Output logs of squad changes (someone joining, leaving etc)
        /// </summary>
        Squads = 1 << 4,

        /// <summary>
        /// Output logs of kills/giveups/revives.
        /// </summary>
        KillsAndSpawns = 1 << 5,

        /// <summary>
        /// Output logs of role changes (player changing role to medic, support etc).
        /// </summary>
        Roles = 1 << 6,

        /// <summary>
        /// Output logs player's healt changes. (When received damage or healed)
        /// </summary>
        HealtChanges = 1 << 7,

        /// <summary>
        /// Output everything.
        /// </summary>
        All = ulong.MaxValue,
    }
}
