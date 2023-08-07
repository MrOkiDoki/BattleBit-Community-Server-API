using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class GetPlayerStatsEventArgs
    {
        /// <summary>
        /// The player's SteamID
        /// </summary>
        public ulong SteamID { get; init; }

        /// <summary>
        /// The player's stats (which you can modify)
        /// </summary>
        public PlayerStats PlayerStats { get; set; }

        internal GetPlayerStatsEventArgs(ulong steamID, PlayerStats playerStats)
        {
            SteamID = steamID;
            PlayerStats = playerStats;
        }
    }
}