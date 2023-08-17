using System.Text;
using BattleBitAPI.Common;
using CommunityServerAPI.Enums;

namespace CommunityServerAPI;

public class CommandHandler
{
    public async Task handleCommand(MyPlayer player, Command cmd)
    {
        switch (cmd.Action)
        {
            case ActionType.Help:
            {
                player.Message("Available commands:");
                var commands = MyGameServer.ApiCommands.Where(c => !c.AdminOnly || player.IsAdmin).ToList();
                
                StringBuilder messageBuilder = new StringBuilder();
                foreach (var command in commands)
                {
                    messageBuilder.Append($"{command.CommandString} - {command.HelpString}\n");
                }
                string message = messageBuilder.ToString();
                
                player.Message(message);
                break;
            }
            case ActionType.Kill:
            {
                var target = cmd.Message.Split(" ")[1..].Aggregate((a, b) => a + " " + b);
                var targetPlayer = player.GameServer.AllPlayers.ToList().FirstOrDefault(p => p.Name.ToLower().Contains(target.ToLower()) || p.SteamID.ToString().Contains(target));
                
                if (targetPlayer == null)
                {
                    player.Message("Player not found!");
                    break;
                }
                
                targetPlayer.Kill();
                player.Message("Killed player!");
                break;
            }
            case ActionType.Start:
            {
                if (player.GameServer.RoundSettings.State != GameState.WaitingForPlayers)
                {
                    player.Message("Round already started!");
                    break;
                }
                
                player.Message("Starting game!");
                player.GameServer.ForceStartGame();
                player.GameServer.RoundSettings.SecondsLeft = 3;
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