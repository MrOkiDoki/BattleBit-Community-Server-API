using BattleBitAPI.Common;
using System.Numerics;
using BattleBitAPI;
using BattleBitAPI.Server;
using CommunityServerAPI.Enums;

namespace CommunityServerAPI;

public abstract class ApiCommand
{
    public string CommandString;
    public string HelpString;
    public string[] Aliases;
    public bool AdminOnly;

    public virtual Command ChatCommand(MyPlayer player, List<MyPlayer> players, ChatChannel channel, string msg)
    {
        return null;
    }
    
    public Player<MyPlayer> FindPlayerByIdentifier(string identifier, List<MyPlayer> players)
    {
        var player = players.Find(p => p.SteamID.ToString() == identifier || p.Name == identifier);
        return player;
    }
}

public class KillCommand : ApiCommand
{
    public KillCommand()
    {
        CommandString = "/kill";
        HelpString = "Kills a player by name or steamid";
        Aliases = new string[] { "/k" };
        AdminOnly = true;
    }

    public override Command ChatCommand(MyPlayer player, List<MyPlayer> players, ChatChannel channel, string msg)
    {
        var words = msg.Split(" ");
        var target = FindPlayerByIdentifier(words[1], players);
        
        if (target == null)
        {
            return new Command
            {
                Action = ActionType.Kill,
                Executor = player.Name,
                Error = true,
            };
        }
        
        return new Command
        {
            Action = ActionType.Kill,
            Executor = player.Name,
            SteamId = target.SteamID,
            Error = false,
        };
    }
}

public class StartCommand : ApiCommand
{
    public StartCommand()
    {
        CommandString = "/start";
        HelpString = "Starts the game";
        Aliases = new string[] { "/s" };
        AdminOnly = true;
    }
    
    public override Command ChatCommand(MyPlayer player, List<MyPlayer> players, ChatChannel channel, string msg)
    {
        return new Command
        {
            Action = ActionType.Start,
            Executor = player.Name,
            Error = false,
        };
    }
}