using System.Reflection;

namespace BattleBitAPI.Common
{
    public static class Attachments
    {
        // ----- Private Variables ----- 
        private static Dictionary<string, Attachment> mAttachments;

        // ----- Barrels ----- 
        public static readonly Attachment Basic = new Attachment("Basic", AttachmentType.Barrel);
        public static readonly Attachment Compensator = new Attachment("Compensator", AttachmentType.Barrel);
        public static readonly Attachment Heavy = new Attachment("Heavy", AttachmentType.Barrel);
        public static readonly Attachment LongBarrel = new Attachment("Long_Barrel", AttachmentType.Barrel);
        public static readonly Attachment MuzzleBreak = new Attachment("Muzzle_Break", AttachmentType.Barrel);
        public static readonly Attachment Ranger = new Attachment("Ranger", AttachmentType.Barrel);
        public static readonly Attachment SuppressorLong = new Attachment("Suppressor_Long", AttachmentType.Barrel);
        public static readonly Attachment SuppressorShort = new Attachment("Suppressor_Short", AttachmentType.Barrel);
        public static readonly Attachment Tactical = new Attachment("Tactical", AttachmentType.Barrel);
        public static readonly Attachment FlashHider = new Attachment("Flash_Hider", AttachmentType.Barrel);
        public static readonly Attachment Osprey9 = new Attachment("Osprey_9", AttachmentType.Barrel);
        public static readonly Attachment DGN308 = new Attachment("DGN-308", AttachmentType.Barrel);
        public static readonly Attachment VAMB762 = new Attachment("VAMB-762", AttachmentType.Barrel);
        public static readonly Attachment SDN6762 = new Attachment("SDN-6_762", AttachmentType.Barrel);
        public static readonly Attachment NT4556 = new Attachment("NT-4_556", AttachmentType.Barrel);

        // ----- Canted Sights ----- 
        public static readonly Attachment Ironsight = new Attachment("Ironsight", AttachmentType.CantedSight);
        public static readonly Attachment CantedRedDot = new Attachment("Canted_Red_Dot", AttachmentType.CantedSight);
        public static readonly Attachment FYouCanted = new Attachment("FYou_Canted", AttachmentType.CantedSight);
        public static readonly Attachment HoloDot = new Attachment("Holo_Dot", AttachmentType.CantedSight);

        // ----- Scope ----- 
        public static readonly Attachment _6xScope = new Attachment("6x_Scope", AttachmentType.MainSight);
        public static readonly Attachment _8xScope = new Attachment("8x_Scope", AttachmentType.MainSight);
        public static readonly Attachment _15xScope = new Attachment("15x_Scope", AttachmentType.MainSight);
        public static readonly Attachment _20xScope = new Attachment("20x_Scope", AttachmentType.MainSight);
        public static readonly Attachment PTR40Hunter = new Attachment("PTR-40_Hunter", AttachmentType.MainSight);
        public static readonly Attachment _1P78 = new Attachment("1P78", AttachmentType.MainSight);
        public static readonly Attachment Acog = new Attachment("Acog", AttachmentType.MainSight);
        public static readonly Attachment M125 = new Attachment("M_125", AttachmentType.MainSight);
        public static readonly Attachment Prisma = new Attachment("Prisma", AttachmentType.MainSight);
        public static readonly Attachment Slip = new Attachment("Slip", AttachmentType.MainSight);
        public static readonly Attachment PistolDeltaSight = new Attachment("Pistol_Delta_Sight", AttachmentType.MainSight);
        public static readonly Attachment PistolRedDot = new Attachment("Pistol_Red_Dot", AttachmentType.MainSight);
        public static readonly Attachment AimComp = new Attachment("Aim_Comp", AttachmentType.MainSight);
        public static readonly Attachment Holographic = new Attachment("Holographic", AttachmentType.MainSight);
        public static readonly Attachment Kobra = new Attachment("Kobra", AttachmentType.MainSight);
        public static readonly Attachment OKP7 = new Attachment("OKP7", AttachmentType.MainSight);
        public static readonly Attachment PKAS = new Attachment("PK-AS", AttachmentType.MainSight);
        public static readonly Attachment RedDot = new Attachment("Red_Dot", AttachmentType.MainSight);
        public static readonly Attachment Reflex = new Attachment("Reflex", AttachmentType.MainSight);
        public static readonly Attachment Strikefire = new Attachment("Strikefire", AttachmentType.MainSight);
        public static readonly Attachment Razor = new Attachment("Razor", AttachmentType.MainSight);
        public static readonly Attachment Flir = new Attachment("Flir", AttachmentType.MainSight);
        public static readonly Attachment Echo = new Attachment("Echo", AttachmentType.MainSight);
        public static readonly Attachment TRI4X32 = new Attachment("TRI4X32", AttachmentType.MainSight);
        public static readonly Attachment FYouSight = new Attachment("FYou_Sight", AttachmentType.MainSight);
        public static readonly Attachment HoloPK120 = new Attachment("Holo_PK-120", AttachmentType.MainSight);
        public static readonly Attachment Pistol8xScope = new Attachment("Pistol_8x_Scope", AttachmentType.MainSight);
        public static readonly Attachment BurrisAR332 = new Attachment("BurrisAR332", AttachmentType.MainSight);
        public static readonly Attachment HS401G5 = new Attachment("HS401G5", AttachmentType.MainSight);

        // ----- Top Scope ----- 
        public static readonly Attachment DeltaSightTop = new Attachment("Delta_Sight_Top", AttachmentType.TopSight);
        public static readonly Attachment RedDotTop = new Attachment("Red_Dot_Top", AttachmentType.TopSight);
        public static readonly Attachment CRedDotTop = new Attachment("C_Red_Dot_Top", AttachmentType.TopSight);
        public static readonly Attachment FYouTop = new Attachment("FYou_Top", AttachmentType.TopSight);

        // ----- Under Rails ----- 
        public static readonly Attachment AngledGrip = new Attachment("Angled_Grip", AttachmentType.UnderRail);
        public static readonly Attachment Bipod = new Attachment("Bipod", AttachmentType.UnderRail);
        public static readonly Attachment VerticalGrip = new Attachment("Vertical_Grip", AttachmentType.UnderRail);
        public static readonly Attachment StubbyGrip = new Attachment("Stubby_Grip", AttachmentType.UnderRail);
        public static readonly Attachment StabilGrip = new Attachment("Stabil_Grip", AttachmentType.UnderRail);
        public static readonly Attachment VerticalSkeletonGrip = new Attachment("Vertical_Skeleton_Grip", AttachmentType.UnderRail);
        public static readonly Attachment FABDTFG = new Attachment("FAB-DTFG", AttachmentType.UnderRail);
        public static readonly Attachment MagpulAngled = new Attachment("Magpul_Angled", AttachmentType.UnderRail);
        public static readonly Attachment BCMGunFighter = new Attachment("BCM-Gun_Fighter", AttachmentType.UnderRail);
        public static readonly Attachment ShiftShortAngledGrip = new Attachment("Shift_Short_Angled_Grip", AttachmentType.UnderRail);
        public static readonly Attachment SE5Grip = new Attachment("SE-5_Grip", AttachmentType.UnderRail);
        public static readonly Attachment RK6Foregrip = new Attachment("RK-6_Foregrip", AttachmentType.UnderRail);
        public static readonly Attachment HeraCQRFront = new Attachment("HeraCQR_Front", AttachmentType.UnderRail);
        public static readonly Attachment B25URK = new Attachment("B-25URK", AttachmentType.UnderRail);
        public static readonly Attachment VTACUVGTacticalGrip = new Attachment("VTAC_UVG_TacticalGrip", AttachmentType.UnderRail);

        // ----- Side Rails ----- 
        public static readonly Attachment Flashlight = new Attachment("Flashlight", AttachmentType.SideRail);
        public static readonly Attachment Rangefinder = new Attachment("Rangefinder", AttachmentType.SideRail);
        public static readonly Attachment Redlaser = new Attachment("Redlaser", AttachmentType.SideRail);
        public static readonly Attachment TacticalFlashlight = new Attachment("Tactical_Flashlight", AttachmentType.SideRail);
        public static readonly Attachment Greenlaser = new Attachment("Greenlaser", AttachmentType.SideRail);
        public static readonly Attachment Searchlight = new Attachment("Searchlight", AttachmentType.SideRail);

        // ----- Bolts ----- 
        public static readonly Attachment BoltActionA = new Attachment("Bolt_Action_A", AttachmentType.Bolt);
        public static readonly Attachment BoltActionB = new Attachment("Bolt_Action_B", AttachmentType.Bolt);
        public static readonly Attachment BoltActionC = new Attachment("Bolt_Action_C", AttachmentType.Bolt);
        public static readonly Attachment BoltActionD = new Attachment("Bolt_Action_D", AttachmentType.Bolt);
        public static readonly Attachment BoltActionE = new Attachment("Bolt_Action_E", AttachmentType.Bolt);

        // ----- Public Calls ----- 
        public static bool TryFind(string name, out Attachment item)
        {
            return mAttachments.TryGetValue(name, out item);
        }

        // ----- Init ----- 
        static Attachments()
        {
            var members = typeof(Attachments).GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            mAttachments = new Dictionary<string, Attachment>(members.Length);
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = ((FieldInfo)memberInfo);
                    if (field.FieldType == typeof(Attachment))
                    {
                        var att = (Attachment)field.GetValue(null);
                        mAttachments.Add(att.Name, att);
                    }
                }
            }
        }
    }
}
