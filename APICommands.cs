using System.Numerics;
using BattleBitAPI.Common;

namespace CommunityServerAPI;

public abstract class APICommand
{
    public Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        return null;
    }
}

public class HealCommand : APICommand
{
    public string CommandPrefix = "!heal";

    public string Help =
        "'steamid' 'amount': Heals specific player the specified amount";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.Heal,
            Amount = int.Parse(splits[2]),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class KillCommand : APICommand
{
    public string CommandPrefix = "!kill";
    public string Help = "'steamid': Kills specific player";

    public new static Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.Kill,
            Amount = 0,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class GrenadeCommand : APICommand
{
    public string CommandPrefix = "!grenade";
    public string Help = "'steamid': spawns live grenade on specific player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.Grenade,
            Amount = 0,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class TeleportCommand : APICommand
{
    public string CommandPrefix = "!teleport";
    public string Help = "'steamid' 'vector': tps specific player to vector location";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var vectorStr = splits[2].Split(",");
        var vector = new Vector3
        {
            X = Convert.ToSingle(vectorStr[0]),
            Y = Convert.ToSingle(vectorStr[1]),
            Z = Convert.ToSingle(vectorStr[1])
        };

        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.Teleport,
            Amount = 0,
            Location = vector,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class SpeedCommand : APICommand
{
    public string CommandPrefix = "!speed";

    public string Help =
        "'steamid' 'amount': Sets speed multiplier of specific player the specified amount";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.Help,
            Amount = int.Parse(splits[2]),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ChangeAttachmentCommand : APICommand
{
    public string CommandPrefix = "!changeAttachment";

    public string Help =
        "'steamid' 'pri=Attachment' 'sec=Attachment': the attachements of specific player the specified amount";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.ChangeAttachement,
            Amount = int.Parse(splits[2]),
            AttachmentChange = Utility.ParseAttachments(splits),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ChangeWeaponCommand : APICommand
{
    public string CommandPrefix = "!changeWeapon";

    public string Help =
        "'steamid' 'pri=Weapon' 'sec=Weapon': the weapons of specific player the specified amount";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.ChangeAttachement,
            Amount = int.Parse(splits[2]),
            AttachmentChange = Utility.ParseAttachments(splits),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ForceStartCommand : APICommand
{
    public string CommandPrefix = "!forceStart";

    public string Help =
        ": forces the game to start";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var c = new Command
        {
            Action = ActionType.Start,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class HelpCommand : APICommand
{
    public string CommandPrefix = "!help";

    public string Help =
        ": lists all commands";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var c = new Command
        {
            Action = ActionType.Help,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class RevealCommand : APICommand
{
    public string CommandPrefix = "!reveal";

    public string Help =
        "'steamid': reveal information about the specified player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.Reveal,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ChangeDamageCommand : APICommand
{
    public string CommandPrefix = "!changeDamage";

    public string Help =
        "'steamid' 'amount': change the damage of the specified player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.ChangeDamage,
            Amount = int.Parse(splits[2]),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ChangeReceivedDamageCommand : APICommand
{
    public string CommandPrefix = "!changeReceivedDamage";

    public string Help =
        "'steamid' 'amount': change the received damage of the specified player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.ChangeReceivedDamage,
            Amount = int.Parse(splits[2]),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ChangeAmmoCommand : APICommand
{
    public string CommandPrefix = "!changeAmmo";

    public string Help =
        "'steamid' 'amount': change the ammo of the specified player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.ChangeAmmo,
            Amount = int.Parse(splits[2]),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class SetStreamerCommand : APICommand
{
    public string CommandPrefix = "!setStreamer";

    public string Help =
        "'steamid': set the specified player as the streamer";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.SetStreamer,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class RemoveStreamerCommand : APICommand
{
    public string CommandPrefix = "!rmStreamer";

    public string Help =
        "'steamid': remove the streamer status from the specified player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.RemoveStreamer,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class OpCommand : APICommand
{
    public string CommandPrefix = "!op";

    public string Help =
        "'steamid': grant operator privileges to the specified player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.GrantOP,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class DeopCommand : APICommand
{
    public string CommandPrefix = "!deop";

    public string Help =
        "'steamid': revoke operator privileges from the specified player";

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.RevokeOP,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}