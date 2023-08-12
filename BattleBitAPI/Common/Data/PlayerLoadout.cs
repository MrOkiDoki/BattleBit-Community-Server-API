namespace BattleBitAPI.Common
{
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
                    this.FirstAidName = "none";
                else
                    this.FirstAidName = value.Name;
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
                    this.LightGadgetName = "none";
                else
                    this.LightGadgetName = value.Name;
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
                    this.HeavyGadgetName = "none";
                else
                    this.HeavyGadgetName = value.Name;
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
                    this.ThrowableName = "none";
                else
                    this.ThrowableName = value.Name;
            }
        }

        public bool HasGadget(Gadget gadget)
        {
            if (this.FirstAid == gadget)
                return true;
            if (this.LightGadget == gadget)
                return true;
            if (this.HeavyGadget == gadget)
                return true;
            if (this.Throwable == gadget)
                return true;
            return false;
        }

        public void Write(Common.Serialization.Stream ser)
        {
            this.PrimaryWeapon.Write(ser);
            this.SecondaryWeapon.Write(ser);
            ser.WriteStringItem(this.FirstAidName);
            ser.WriteStringItem(this.LightGadgetName);
            ser.WriteStringItem(this.HeavyGadgetName);
            ser.WriteStringItem(this.ThrowableName);

            ser.Write(this.PrimaryExtraMagazines);
            ser.Write(this.SecondaryExtraMagazines);
            ser.Write(this.FirstAidExtra);
            ser.Write(this.LightGadgetExtra);
            ser.Write(this.HeavyGadgetExtra);
            ser.Write(this.ThrowableExtra);
        }
        public void Read(Common.Serialization.Stream ser)
        {
            this.PrimaryWeapon.Read(ser);
            this.SecondaryWeapon.Read(ser);
            ser.TryReadString(out this.FirstAidName);
            ser.TryReadString(out this.LightGadgetName);
            ser.TryReadString(out this.HeavyGadgetName);
            ser.TryReadString(out this.ThrowableName);

            this.PrimaryExtraMagazines = ser.ReadInt8();
            this.SecondaryExtraMagazines = ser.ReadInt8();
            this.FirstAidExtra = ser.ReadInt8();
            this.LightGadgetExtra = ser.ReadInt8();
            this.HeavyGadgetExtra = ser.ReadInt8();
            this.ThrowableExtra = ser.ReadInt8();
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
                    this.ToolName = "none";
                else
                    this.ToolName = value.Name;
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
                    this.MainSightName = "none";
                else
                    this.MainSightName = value.Name;
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
                    this.TopSightName = "none";
                else
                    this.TopSightName = value.Name;
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
                    this.CantedSightName = "none";
                else
                    this.CantedSightName = value.Name;
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
                    this.BarrelName = "none";
                else
                    this.BarrelName = value.Name;
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
                    this.SideRailName = "none";
                else
                    this.SideRailName = value.Name;
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
                    this.UnderRailName = "none";
                else
                    this.UnderRailName = value.Name;
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
                    this.BoltActionName = "none";
                else
                    this.BoltActionName = value.Name;
            }
        }

        public bool HasAttachment(Attachment attachment)
        {
            switch (attachment.AttachmentType)
            {
                case AttachmentType.MainSight:
                    return this.MainSight == attachment;
                case AttachmentType.TopSight:
                    return this.TopSight == attachment;
                case AttachmentType.CantedSight:
                    return this.CantedSight == attachment;
                case AttachmentType.Barrel:
                    return this.Barrel == attachment;
                case AttachmentType.UnderRail:
                    return this.UnderRail == attachment;
                case AttachmentType.SideRail:
                    return this.SideRail == attachment;
                case AttachmentType.Bolt:
                    return this.BoltAction == attachment;
            }
            return false;
        }
        public void SetAttachment(Attachment attachment)
        {
            switch (attachment.AttachmentType)
            {
                case AttachmentType.MainSight:
                    this.MainSight = attachment;
                    break;
                case AttachmentType.TopSight:
                    this.TopSight = attachment;
                    break;
                case AttachmentType.CantedSight:
                    this.CantedSight = attachment;
                    break;
                case AttachmentType.Barrel:
                    this.Barrel = attachment;
                    break;
                case AttachmentType.UnderRail:
                    this.UnderRail = attachment;
                    break;
                case AttachmentType.SideRail:
                    this.SideRail = attachment;
                    break;
                case AttachmentType.Bolt:
                    this.BoltAction = attachment;
                    break;
            }
        }

        public void Write(Common.Serialization.Stream ser)
        {
            ser.WriteStringItem(this.ToolName);
            ser.WriteStringItem(this.MainSightName);
            ser.WriteStringItem(this.TopSightName);
            ser.WriteStringItem(this.CantedSightName);
            ser.WriteStringItem(this.BarrelName);
            ser.WriteStringItem(this.SideRailName);
            ser.WriteStringItem(this.UnderRailName);
            ser.WriteStringItem(this.BoltActionName);
            ser.Write(this.SkinIndex);
            ser.Write(this.MagazineIndex);
        }
        public void Read(Common.Serialization.Stream ser)
        {
            ser.TryReadString(out this.ToolName);
            ser.TryReadString(out this.MainSightName);
            ser.TryReadString(out this.TopSightName);
            ser.TryReadString(out this.CantedSightName);
            ser.TryReadString(out this.BarrelName);
            ser.TryReadString(out this.SideRailName);
            ser.TryReadString(out this.UnderRailName);
            ser.TryReadString(out this.BoltActionName);
            this.SkinIndex = ser.ReadInt8();
            this.MagazineIndex = ser.ReadInt8();
        }
    }
}
