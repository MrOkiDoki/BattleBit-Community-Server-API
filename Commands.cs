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

    public virtual Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        return null;
    }
}

public class HelpCommand : ApiCommand
{
    public HelpCommand()
    {
        CommandString = "/help";
        HelpString = "Shows this help message";
        Aliases = new string[] { "/h" };
        AdminOnly = false;
    }

    public override Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        return new Command()
        {
            Action = ActionType.Help,
            Executor = player.Name,
            Error = false,
        };
    }
}

public class StatsCommand : ApiCommand
{
    public StatsCommand()
    {
        CommandString = "/stats";
        HelpString = "Display your stats";
        Aliases = new string[] { "/s" };
        AdminOnly = false;
    }
    
    public override Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        return new Command()
        {
            Action = ActionType.Stats,
            Executor = player.Name,
            Error = false,
        };
    }
}

public class KillCommand : ApiCommand
{
    public KillCommand()
    {
        CommandString = "/kill";
        HelpString = "Kill player by name or steamid";
        Aliases = new string[] { "/k" };
        AdminOnly = true;
    }

    public override Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        return new Command()
        {
            Action = ActionType.Kill,
            Executor = player.Name,
            Message = msg,
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
    
    public override Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        return new Command
        {
            Action = ActionType.Start,
            Executor = player.Name,
            Error = false,
        };
    }
}