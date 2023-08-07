using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class SavingPlayerStatsEventArgs
    {
        /// <summary>
        /// The player's SteamID
        /// </summary>
        public ulong SteamID { get; init; }

        /// <summary>
        /// The player's stats
        /// </summary>
        public PlayerStats PlayerStats { get; init; }

        internal SavingPlayerStatsEventArgs(ulong steamID, PlayerStats playerStats)
        {
            SteamID = steamID;
            PlayerStats = playerStats;
        }
    }
}