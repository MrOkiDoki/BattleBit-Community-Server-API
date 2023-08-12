using BattleBitAPI.Common;

namespace CommunityServerAPI;

public class Utility
{
    public static List<Attachment> ParseAttachments(string[] splits)
    {
        var attachments = new List<Attachment>();
        var s = splits.Skip(3);
        foreach (var st in s)
        {
            if(Attachments.TryFind(st, out var attach))
            {
                attachments.Add(attach);
            }
            else
            {
                attachments.Add(default);
            }
                
        }

        return attachments;
    }
    
    public static List<Weapon> ParseWeapons(string[] splits)
    {
        var weapons = new List<Weapon>();
        var s = splits.Skip(3);
        foreach (var st in s)
        {
            if(Weapons.TryFind(st, out var weapon))
            {
                weapons.Add(weapon);
            }
            else
            {
                weapons.Add(default);
            }
                
        }

        return weapons;
    }
    
    public static List<Gadget> ParseGadgets(string[] splits)
    {
        var gadgets = new List<Gadget>();
        var s = splits.Skip(3);
        foreach (var st in s)
        {
            if(Gadgets.TryFind(st, out var gadget))
            {
                gadgets.Add(gadget);
            }
            else
            {
                gadgets.Add(default);
            }
                
        }

        return gadgets;
    }
}