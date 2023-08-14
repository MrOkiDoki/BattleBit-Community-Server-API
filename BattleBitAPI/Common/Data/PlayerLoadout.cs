using Stream = BattleBitAPI.Common.Serialization.Stream;

namespace BattleBitAPI.Common;

public struct PlayerLoadout
{
    public WeaponItem PrimaryWeapon;
    public WeaponItem SecondaryWeapon;
    public string FirstAidName;
    public string LightGadgetName;
    public string HeavyGadgetName;
    public string ThrowableName;

    public byte PrimaryExtraMagazines;
    public byte SecondaryExtraMagazines;
    public byte FirstAidExtra;
    public byte LightGadgetExtra;
    public byte HeavyGadgetExtra;
    public byte ThrowableExtra;

    public Gadget FirstAid
    {
        get
        {
            if (Gadgets.TryFind(FirstAidName, out var gadget))
                return gadget;
            return null;
        }
        set
        {
            if (value == null)
                FirstAidName = "none";
            else
                FirstAidName = value.Name;
        }
    }

    public Gadget LightGadget
    {
        get
        {
            if (Gadgets.TryFind(LightGadgetName, out var gadget))
                return gadget;
            return null;
        }
        set
        {
            if (value == null)
                LightGadgetName = "none";
            else
                LightGadgetName = value.Name;
        }
    }

    public Gadget HeavyGadget
    {
        get
        {
            if (Gadgets.TryFind(HeavyGadgetName, out var gadget))
                return gadget;
            return null;
        }
        set
        {
            if (value == null)
                HeavyGadgetName = "none";
            else
                HeavyGadgetName = value.Name;
        }
    }

    public Gadget Throwable
    {
        get
        {
            if (Gadgets.TryFind(ThrowableName, out var gadget))
                return gadget;
            return null;
        }
        set
        {
            if (value == null)
                ThrowableName = "none";
            else
                ThrowableName = value.Name;
        }
    }

    public bool HasGadget(Gadget gadget)
    {
        if (FirstAid == gadget)
            return true;
        if (LightGadget == gadget)
            return true;
        if (HeavyGadget == gadget)
            return true;
        if (Throwable == gadget)
            return true;
        return false;
    }

    public void Write(Stream ser)
    {
        PrimaryWeapon.Write(ser);
        SecondaryWeapon.Write(ser);
        ser.WriteStringItem(FirstAidName);
        ser.WriteStringItem(LightGadgetName);
        ser.WriteStringItem(HeavyGadgetName);
        ser.WriteStringItem(ThrowableName);

        ser.Write(PrimaryExtraMagazines);
        ser.Write(SecondaryExtraMagazines);
        ser.Write(FirstAidExtra);
        ser.Write(LightGadgetExtra);
        ser.Write(HeavyGadgetExtra);
        ser.Write(ThrowableExtra);
    }

    public void Read(Stream ser)
    {
        PrimaryWeapon.Read(ser);
        SecondaryWeapon.Read(ser);
        ser.TryReadString(out FirstAidName);
        ser.TryReadString(out LightGadgetName);
        ser.TryReadString(out HeavyGadgetName);
        ser.TryReadString(out ThrowableName);

        PrimaryExtraMagazines = ser.ReadInt8();
        SecondaryExtraMagazines = ser.ReadInt8();
        FirstAidExtra = ser.ReadInt8();
        LightGadgetExtra = ser.ReadInt8();
        HeavyGadgetExtra = ser.ReadInt8();
        ThrowableExtra = ser.ReadInt8();
    }
}

public struct WeaponItem
{
    public string ToolName;
    public string MainSightName;
    public string TopSightName;
    public string CantedSightName;
    public string BarrelName;
    public string SideRailName;
    public string UnderRailName;
    public string BoltActionName;
    public byte SkinIndex;
    public byte MagazineIndex;

    public Weapon Tool
    {
        get
        {
            if (Weapons.TryFind(ToolName, out var weapon))
                return weapon;
            return null;
        }
        set
        {
            if (value == null)
                ToolName = "none";
            else
                ToolName = value.Name;
        }
    }

    public Attachment MainSight
    {
        get
        {
            if (Attachments.TryFind(MainSightName, out var attachment))
                return attachment;
            return null;
        }
        set
        {
            if (value == null)
                MainSightName = "none";
            else
                MainSightName = value.Name;
        }
    }

    public Attachment TopSight
    {
        get
        {
            if (Attachments.TryFind(TopSightName, out var attachment))
                return attachment;
            return null;
        }
        set
        {
            if (value == null)
                TopSightName = "none";
            else
                TopSightName = value.Name;
        }
    }

    public Attachment CantedSight
    {
        get
        {
            if (Attachments.TryFind(CantedSightName, out var attachment))
                return attachment;
            return null;
        }
        set
        {
            if (value == null)
                CantedSightName = "none";
            else
                CantedSightName = value.Name;
        }
    }

    public Attachment Barrel
    {
        get
        {
            if (Attachments.TryFind(BarrelName, out var attachment))
                return attachment;
            return null;
        }
        set
        {
            if (value == null)
                BarrelName = "none";
            else
                BarrelName = value.Name;
        }
    }

    public Attachment SideRail
    {
        get
        {
            if (Attachments.TryFind(SideRailName, out var attachment))
                return attachment;
            return null;
        }
        set
        {
            if (value == null)
                SideRailName = "none";
            else
                SideRailName = value.Name;
        }
    }

    public Attachment UnderRail
    {
        get
        {
            if (Attachments.TryFind(UnderRailName, out var attachment))
                return attachment;
            return null;
        }
        set
        {
            if (value == null)
                UnderRailName = "none";
            else
                UnderRailName = value.Name;
        }
    }

    public Attachment BoltAction
    {
        get
        {
            if (Attachments.TryFind(BoltActionName, out var attachment))
                return attachment;
            return null;
        }
        set
        {
            if (value == null)
                BoltActionName = "none";
            else
                BoltActionName = value.Name;
        }
    }

    public bool HasAttachment(Attachment attachment)
    {
        switch (attachment.AttachmentType)
        {
            case AttachmentType.MainSight:
                return MainSight == attachment;
            case AttachmentType.TopSight:
                return TopSight == attachment;
            case AttachmentType.CantedSight:
                return CantedSight == attachment;
            case AttachmentType.Barrel:
                return Barrel == attachment;
            case AttachmentType.UnderRail:
                return Barrel == attachment;
            case AttachmentType.SideRail:
                return SideRail == attachment;
            case AttachmentType.Bolt:
                return BoltAction == attachment;
        }

        return false;
    }

    public void SetAttachment(Attachment attachment)
    {
        switch (attachment.AttachmentType)
        {
            case AttachmentType.MainSight:
                MainSight = attachment;
                break;
            case AttachmentType.TopSight:
                TopSight = attachment;
                break;
            case AttachmentType.CantedSight:
                CantedSight = attachment;
                break;
            case AttachmentType.Barrel:
                Barrel = attachment;
                break;
            case AttachmentType.UnderRail:
                Barrel = attachment;
                break;
            case AttachmentType.SideRail:
                SideRail = attachment;
                break;
            case AttachmentType.Bolt:
                BoltAction = attachment;
                break;
        }
    }

    public void Write(Stream ser)
    {
        ser.WriteStringItem(ToolName);
        ser.WriteStringItem(MainSightName);
        ser.WriteStringItem(TopSightName);
        ser.WriteStringItem(CantedSightName);
        ser.WriteStringItem(BarrelName);
        ser.WriteStringItem(SideRailName);
        ser.WriteStringItem(UnderRailName);
        ser.WriteStringItem(BoltActionName);
        ser.Write(SkinIndex);
        ser.Write(MagazineIndex);
    }

    public void Read(Stream ser)
    {
        ser.TryReadString(out ToolName);
        ser.TryReadString(out MainSightName);
        ser.TryReadString(out TopSightName);
        ser.TryReadString(out CantedSightName);
        ser.TryReadString(out BarrelName);
        ser.TryReadString(out SideRailName);
        ser.TryReadString(out UnderRailName);
        ser.TryReadString(out BoltActionName);
        SkinIndex = ser.ReadInt8();
        MagazineIndex = ser.ReadInt8();
    }
}