using BattleBitAPI;
using CommunityServerAPI.Enums;

namespace CommunityServerAPI;

public class CommandHandler
{
    public async Task handleCommand(MyPlayer player, List<MyPlayer> players, Command cmd)
    {
        switch (cmd.Action)
        {
            case ActionType.Kill:
            {
                var target = players.Find(p => p.SteamID == cmd.SteamId);
                if (target == null)
                {
                    player.Message("Player not found!");
                    return;
                }
                
                target.Kill();
                player.Message("Player killed!");
                break;
            }
            case ActionType.Start:
            {
                player.Message("Starting game!");
                player.GameServer.RoundSettings.PlayersToStart = 0;
                break;
            }
            default:
            {
                player.Message("Unknown command!");
                break;
            }
        }
    }
}