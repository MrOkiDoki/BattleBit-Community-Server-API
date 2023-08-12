using System.Numerics;
using BattleBitAPI.Common;

namespace CommunityServerAPI;

 public class APICommand
    {
        public string CommandPrefix = string.Empty;
        public string Help = string.Empty;

        public Command ChatCommand(MyPlayer player, ChatChannel channel, string msg)
        {
            return null;
        }
    }

    public class HealCommand : APICommand
    {
        public new string CommandPrefix = "!heal";

        public new string Help =
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
        public new string CommandPrefix = "!kill";
        public new string Help = "'steamid': Kills specific player";

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
        public new string CommandPrefix = "!grenade";
        public new string Help = "'steamid': spawns live grenade on specific player";

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
        public new string CommandPrefix = "!teleport";
        public new string Help = "'steamid' 'vector': tps specific player to vector location";

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
        public new string CommandPrefix = "!speed";

        public new string Help =
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
        public new string CommandPrefix = "!changeAttachment";

        public new string Help =
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
        public new string CommandPrefix = "!changeWeapon";

        public new string Help =
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
        public new string CommandPrefix = "!forceStart";

        public new string Help =
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
        public new string CommandPrefix = "!help";

        public new string Help =
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
        public new string CommandPrefix = "!reveal";

        public new string Help =
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
        public new string CommandPrefix = "!changeDamage";

        public new string Help =
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
        public new string CommandPrefix = "!changeReceivedDamage";

        public new string Help =
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
    public new string CommandPrefix = "!changeAmmo";

    public new string Help =
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
    public new string CommandPrefix = "!setStreamer";

    public new string Help =
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
    public new string CommandPrefix = "!rmStreamer";

    public new string Help =
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
    public new string CommandPrefix = "!op";

    public new string Help =
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
    public new string CommandPrefix = "!deop";

    public new string Help =
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