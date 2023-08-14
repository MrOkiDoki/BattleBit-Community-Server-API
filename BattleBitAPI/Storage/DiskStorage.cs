using BattleBitAPI.Common;

namespace BattleBitAPI.Storage;

public class DiskStorage : IPlayerStatsDatabase
{
    private readonly string mDirectory;

    public DiskStorage(string directory)
    {
        var info = new DirectoryInfo(directory);
        if (!info.Exists)
            info.Create();

        mDirectory = info.FullName + Path.DirectorySeparatorChar;
    }

    public async Task<PlayerStats> GetPlayerStatsOf(ulong steamID)
    {
        var file = mDirectory + steamID + ".data";
        if (File.Exists(file))
            try
            {
                var data = await File.ReadAllBytesAsync(file);
                return new PlayerStats(data);
            }
            catch
            {
            }

        return null;
    }

    public async Task SavePlayerStatsOf(ulong steamID, PlayerStats stats)
    {
        var file = mDirectory + steamID + ".data";
        try
        {
            await File.WriteAllBytesAsync(file, stats.SerializeToByteArray());
        }
        catch
        {
        }
    }
}