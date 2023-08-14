using System.Reflection;

namespace BattleBitAPI.Common;

public static class Gadgets
{
    // ----- Private Variables ----- 
    private static readonly Dictionary<string, Gadget> mGadgets;

    // ----- Public Variables ----- 
    public static readonly Gadget Bandage = new("Bandage");
    public static readonly Gadget Binoculars = new("Binoculars");
    public static readonly Gadget RangeFinder = new("Range Finder");
    public static readonly Gadget RepairTool = new("Repair Tool");
    public static readonly Gadget C4 = new("C4");
    public static readonly Gadget Claymore = new("Claymore");
    public static readonly Gadget M320SmokeGrenadeLauncher = new("M320 Smoke Grenade Launcher");
    public static readonly Gadget SmallAmmoKit = new("Small Ammo Kit");
    public static readonly Gadget AntiPersonnelMine = new("Anti Personnel Mine");
    public static readonly Gadget AntiVehicleMine = new("Anti Vehicle Mine");
    public static readonly Gadget MedicKit = new("Medic Kit");
    public static readonly Gadget Rpg7HeatExplosive = new("Rpg7 Heat Explosive");
    public static readonly Gadget RiotShield = new("Riot Shield");
    public static readonly Gadget FragGrenade = new("Frag Grenade");
    public static readonly Gadget ImpactGrenade = new("Impact Grenade");
    public static readonly Gadget AntiVehicleGrenade = new("Anti Vehicle Grenade");
    public static readonly Gadget SmokeGrenadeBlue = new("Smoke Grenade Blue");
    public static readonly Gadget SmokeGrenadeGreen = new("Smoke Grenade Green");
    public static readonly Gadget SmokeGrenadeRed = new("Smoke Grenade Red");
    public static readonly Gadget SmokeGrenadeWhite = new("Smoke Grenade White");
    public static readonly Gadget Flare = new("Flare");
    public static readonly Gadget SledgeHammer = new("Sledge Hammer");
    public static readonly Gadget AdvancedBinoculars = new("Advanced Binoculars");
    public static readonly Gadget Mdx201 = new("Mdx 201");
    public static readonly Gadget BinoSoflam = new("Bino Soflam");
    public static readonly Gadget HeavyAmmoKit = new("Heavy Ammo Kit");
    public static readonly Gadget Rpg7Pgo7Tandem = new("Rpg7 Pgo7 Tandem");
    public static readonly Gadget Rpg7Pgo7HeatExplosive = new("Rpg7 Pgo7 Heat Explosive");
    public static readonly Gadget Rpg7Pgo7Fragmentation = new("Rpg7 Pgo7 Fragmentation");
    public static readonly Gadget Rpg7Fragmentation = new("Rpg7 Fragmentation");
    public static readonly Gadget GrapplingHook = new("Grappling Hook");
    public static readonly Gadget AirDrone = new("Air Drone");
    public static readonly Gadget Flashbang = new("Flashbang");
    public static readonly Gadget Pickaxe = new("Pickaxe");
    public static readonly Gadget SuicideC4 = new("SuicideC4");
    public static readonly Gadget SledgeHammerSkinA = new("Sledge Hammer SkinA");
    public static readonly Gadget SledgeHammerSkinB = new("Sledge Hammer SkinB");
    public static readonly Gadget SledgeHammerSkinC = new("Sledge Hammer SkinC");
    public static readonly Gadget PickaxeIronPickaxe = new("Pickaxe IronPickaxe");

    // ----- Init ----- 
    static Gadgets()
    {
        var members = typeof(Gadgets).GetMembers(BindingFlags.Public | BindingFlags.Static);
        mGadgets = new Dictionary<string, Gadget>(members.Length);
        foreach (var memberInfo in members)
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)memberInfo;
                if (field.FieldType == typeof(Gadget))
                {
                    var gad = (Gadget)field.GetValue(null);
                    mGadgets.Add(gad.Name, gad);
                }
            }
    }

    // ----- Public Calls ----- 
    public static bool TryFind(string name, out Gadget item)
    {
        return mGadgets.TryGetValue(name, out item);
    }
}