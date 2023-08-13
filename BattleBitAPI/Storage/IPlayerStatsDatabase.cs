using BattleBitAPI.Common;
using BattleBitAPI.Common.Data;

namespace BattleBitAPI.Storage
{
    public interface IPlayerStatsDatabase
    {
        public Task<PlayerStats> GetPlayerStatsOf(ulong steamID);
        public Task SavePlayerStatsOf(ulong steamID, PlayerStats stats);
    }
}
