using System.Numerics;
using BattleBitAPI.Common;

namespace CommunityServerAPI;

public abstract class APICommand
{
    public string CommandPrefix;
    public string Help;

    public Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        return null;
    }
}

public class HealCommand : APICommand
{
    public HealCommand()
    {
        CommandPrefix = "!heal";

        Help =
            "'steamid' 'amount': Heals specific player the specified amount";
    }


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
    public KillCommand()
    {
        CommandPrefix = "!kill";
        Help = "'steamid': Kills specific player";
    }


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
    public TeleportCommand()
    {
        CommandPrefix = "!teleport";
        Help = "'steamid' 'vector': Teleports specific player to vector location";
    }

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var vectorStr = splits[2].Split(",");
        var vector = new Vector3
        {
            X = Convert.ToSingle(vectorStr[0]),
            Y = Convert.ToSingle(vectorStr[1]),
            Z = Convert.ToSingle(vectorStr[2]) // Fix the index here to [2]
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
    public SpeedCommand()
    {
        CommandPrefix = "!speed";
        Help = "'steamid' 'amount': Sets speed multiplier of specific player to the specified amount";
    }

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.Speed,
            Amount = int.Parse(splits[2]),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ChangeAttachmentCommand : APICommand
{
    public ChangeAttachmentCommand()
    {
        CommandPrefix = "!changeAttachment";
        Help = "'steamid' 'pri=Attachment' 'sec=Attachment': Change attachments of specific player";
    }

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.ChangeAttachement,
            Amount = 0, // Not sure what this value represents, please adjust accordingly
            AttachmentChange = Utility.ParseAttachments(splits),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ChangeWeaponCommand : APICommand
{
    public ChangeWeaponCommand()
    {
        CommandPrefix = "!changeWeapon";
        Help = "'steamid' 'pri=Weapon' 'sec=Weapon': Change weapons of specific player";
    }

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]),
            Action = ActionType.ChangeWeapon,
            Amount = 0, // Not sure what this value represents, please adjust accordingly
            AttachmentChange = Utility.ParseAttachments(splits),
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class ForceStartCommand : APICommand
{
    public ForceStartCommand()
    {
        CommandPrefix = "!forceStart";
        Help = ": Forces the game to start";
    }

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var c = new Command
        {
            Action = ActionType.Start,
            StreamerId = player.SteamID,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class HelpCommand : APICommand
{
    public HelpCommand()
    {
        CommandPrefix = "!help";
        Help = ": Lists all commands";
    }

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var c = new Command
        {
            Action = ActionType.Help,
            StreamerId = player.SteamID,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class RevealCommand : APICommand
{
    public RevealCommand()
    {
        CommandPrefix = "!reveal";
        Help = "'steamid': Reveal information about the specified player";
    }

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
    public ChangeDamageCommand()
    {
        CommandPrefix = "!changeDamage";
        Help = "'steamid' 'amount': Change the damage of the specified player";
    }

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
    public ChangeReceivedDamageCommand()
    {
        CommandPrefix = "!changeReceivedDamage";
        Help = "'steamid' 'amount': Change the received damage of the specified player";
    }

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
    public ChangeAmmoCommand()
    {
        CommandPrefix = "!changeAmmo";
        Help = "'steamid' 'amount': Change the ammo of the specified player";
    }

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
    public SetStreamerCommand()
    {
        CommandPrefix = "!setStreamer";
        Help = "'steamid': Set the specified player as the streamer";
    }

    public new Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
    {
        var splits = msg.Split(" ");
        var c = new Command
        {
            StreamerId = Convert.ToUInt64(splits[1]), // needs fixing, will check if that is already a streamer
            Action = ActionType.SetStreamer,
            ExecutorName = "Chat Test"
        };
        return c;
    }
}

public class RemoveStreamerCommand : APICommand
{
    public RemoveStreamerCommand()
    {
        CommandPrefix = "!rmStreamer";
        Help = "'steamid': Remove the streamer status from the specified player";
    }

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
    public OpCommand()
    {
        CommandPrefix = "!op";
        Help = "'steamid': Grant operator privileges to the specified player";
    }

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
    public DeopCommand()
    {
        CommandPrefix = "!deop";
        Help = "'steamid': Revoke operator privileges from the specified player";
    }

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