using System.Reflection;

namespace BattleBitAPI.Common;

public static class Attachments
{
    // ----- Private Variables ----- 
    private static readonly Dictionary<string, Attachment> mAttachments;

    // ----- Barrels ----- 
    public static readonly Attachment Basic = new("Basic", AttachmentType.Barrel);
    public static readonly Attachment Compensator = new("Compensator", AttachmentType.Barrel);
    public static readonly Attachment Heavy = new("Heavy", AttachmentType.Barrel);
    public static readonly Attachment LongBarrel = new("Long_Barrel", AttachmentType.Barrel);
    public static readonly Attachment MuzzleBreak = new("Muzzle_Break", AttachmentType.Barrel);
    public static readonly Attachment Ranger = new("Ranger", AttachmentType.Barrel);
    public static readonly Attachment SuppressorLong = new("Suppressor_Long", AttachmentType.Barrel);
    public static readonly Attachment SuppressorShort = new("Suppressor_Short", AttachmentType.Barrel);
    public static readonly Attachment Tactical = new("Tactical", AttachmentType.Barrel);
    public static readonly Attachment FlashHider = new("Flash_Hider", AttachmentType.Barrel);
    public static readonly Attachment Osprey9 = new("Osprey_9", AttachmentType.Barrel);
    public static readonly Attachment DGN308 = new("DGN-308", AttachmentType.Barrel);
    public static readonly Attachment VAMB762 = new("VAMB-762", AttachmentType.Barrel);
    public static readonly Attachment SDN6762 = new("SDN-6_762", AttachmentType.Barrel);
    public static readonly Attachment NT4556 = new("NT-4_556", AttachmentType.Barrel);

    // ----- Canted Sights ----- 
    public static readonly Attachment Ironsight = new("Ironsight", AttachmentType.CantedSight);
    public static readonly Attachment CantedRedDot = new("Canted_Red_Dot", AttachmentType.CantedSight);
    public static readonly Attachment FYouCanted = new("FYou_Canted", AttachmentType.CantedSight);
    public static readonly Attachment HoloDot = new("Holo_Dot", AttachmentType.CantedSight);

    // ----- Scope ----- 
    public static readonly Attachment _6xScope = new("6x_Scope", AttachmentType.MainSight);
    public static readonly Attachment _8xScope = new("8x_Scope", AttachmentType.MainSight);
    public static readonly Attachment _15xScope = new("15x_Scope", AttachmentType.MainSight);
    public static readonly Attachment _20xScope = new("20x_Scope", AttachmentType.MainSight);
    public static readonly Attachment PTR40Hunter = new("PTR-40_Hunter", AttachmentType.MainSight);
    public static readonly Attachment _1P78 = new("1P78", AttachmentType.MainSight);
    public static readonly Attachment Acog = new("Acog", AttachmentType.MainSight);
    public static readonly Attachment M125 = new("M_125", AttachmentType.MainSight);
    public static readonly Attachment Prisma = new("Prisma", AttachmentType.MainSight);
    public static readonly Attachment Slip = new("Slip", AttachmentType.MainSight);
    public static readonly Attachment PistolDeltaSight = new("Pistol_Delta_Sight", AttachmentType.MainSight);
    public static readonly Attachment PistolRedDot = new("Pistol_Red_Dot", AttachmentType.MainSight);
    public static readonly Attachment AimComp = new("Aim_Comp", AttachmentType.MainSight);
    public static readonly Attachment Holographic = new("Holographic", AttachmentType.MainSight);
    public static readonly Attachment Kobra = new("Kobra", AttachmentType.MainSight);
    public static readonly Attachment OKP7 = new("OKP7", AttachmentType.MainSight);
    public static readonly Attachment PKAS = new("PK-AS", AttachmentType.MainSight);
    public static readonly Attachment RedDot = new("Red_Dot", AttachmentType.MainSight);
    public static readonly Attachment Reflex = new("Reflex", AttachmentType.MainSight);
    public static readonly Attachment Strikefire = new("Strikefire", AttachmentType.MainSight);
    public static readonly Attachment Razor = new("Razor", AttachmentType.MainSight);
    public static readonly Attachment Flir = new("Flir", AttachmentType.MainSight);
    public static readonly Attachment Echo = new("Echo", AttachmentType.MainSight);
    public static readonly Attachment TRI4X32 = new("TRI4X32", AttachmentType.MainSight);
    public static readonly Attachment FYouSight = new("FYou_Sight", AttachmentType.MainSight);
    public static readonly Attachment HoloPK120 = new("Holo_PK-120", AttachmentType.MainSight);
    public static readonly Attachment Pistol8xScope = new("Pistol_8x_Scope", AttachmentType.MainSight);
    public static readonly Attachment BurrisAR332 = new("BurrisAR332", AttachmentType.MainSight);
    public static readonly Attachment HS401G5 = new("HS401G5", AttachmentType.MainSight);

    // ----- Top Scope ----- 
    public static readonly Attachment DeltaSightTop = new("Delta_Sight_Top", AttachmentType.TopSight);
    public static readonly Attachment RedDotTop = new("Red_Dot_Top", AttachmentType.TopSight);
    public static readonly Attachment CRedDotTop = new("C_Red_Dot_Top", AttachmentType.TopSight);
    public static readonly Attachment FYouTop = new("FYou_Top", AttachmentType.TopSight);

    // ----- Under Rails ----- 
    public static readonly Attachment AngledGrip = new("Angled_Grip", AttachmentType.UnderRail);
    public static readonly Attachment Bipod = new("Bipod", AttachmentType.UnderRail);
    public static readonly Attachment VerticalGrip = new("Vertical_Grip", AttachmentType.UnderRail);
    public static readonly Attachment StubbyGrip = new("Stubby_Grip", AttachmentType.UnderRail);
    public static readonly Attachment StabilGrip = new("Stabil_Grip", AttachmentType.UnderRail);
    public static readonly Attachment VerticalSkeletonGrip = new("Vertical_Skeleton_Grip", AttachmentType.UnderRail);
    public static readonly Attachment FABDTFG = new("FAB-DTFG", AttachmentType.UnderRail);
    public static readonly Attachment MagpulAngled = new("Magpul_Angled", AttachmentType.UnderRail);
    public static readonly Attachment BCMGunFighter = new("BCM-Gun_Fighter", AttachmentType.UnderRail);
    public static readonly Attachment ShiftShortAngledGrip = new("Shift_Short_Angled_Grip", AttachmentType.UnderRail);
    public static readonly Attachment SE5Grip = new("SE-5_Grip", AttachmentType.UnderRail);
    public static readonly Attachment RK6Foregrip = new("RK-6_Foregrip", AttachmentType.UnderRail);
    public static readonly Attachment HeraCQRFront = new("HeraCQR_Front", AttachmentType.UnderRail);
    public static readonly Attachment B25URK = new("B-25URK", AttachmentType.UnderRail);
    public static readonly Attachment VTACUVGTacticalGrip = new("VTAC_UVG_TacticalGrip", AttachmentType.UnderRail);

    // ----- Side Rails ----- 
    public static readonly Attachment Flashlight = new("Flashlight", AttachmentType.SideRail);
    public static readonly Attachment Rangefinder = new("Rangefinder", AttachmentType.SideRail);
    public static readonly Attachment Redlaser = new("Redlaser", AttachmentType.SideRail);
    public static readonly Attachment TacticalFlashlight = new("Tactical_Flashlight", AttachmentType.SideRail);
    public static readonly Attachment Greenlaser = new("Greenlaser", AttachmentType.SideRail);
    public static readonly Attachment Searchlight = new("Searchlight", AttachmentType.SideRail);

    // ----- Bolts ----- 
    public static readonly Attachment BoltActionA = new("Bolt_Action_A", AttachmentType.Bolt);
    public static readonly Attachment BoltActionB = new("Bolt_Action_B", AttachmentType.Bolt);
    public static readonly Attachment BoltActionC = new("Bolt_Action_C", AttachmentType.Bolt);
    public static readonly Attachment BoltActionD = new("Bolt_Action_D", AttachmentType.Bolt);
    public static readonly Attachment BoltActionE = new("Bolt_Action_E", AttachmentType.Bolt);

    // ----- Init ----- 
    static Attachments()
    {
        var members = typeof(Attachments).GetMembers(BindingFlags.Public | BindingFlags.Static);
        mAttachments = new Dictionary<string, Attachment>(members.Length);
        foreach (var memberInfo in members)
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)memberInfo;
                if (field.FieldType == typeof(Attachment))
                {
                    var att = (Attachment)field.GetValue(null);
                    mAttachments.Add(att.Name, att);
                }
            }
    }

    // ----- Public Calls ----- 
    public static bool TryFind(string name, out Attachment item)
    {
        return mAttachments.TryGetValue(name, out item);
    }
}