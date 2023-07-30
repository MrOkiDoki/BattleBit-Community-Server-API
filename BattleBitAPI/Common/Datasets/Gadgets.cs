using System.Reflection;

namespace BattleBitAPI.Common
{
    public static class Gadgets
    {
        // ----- Private Variables ----- 
        private static Dictionary<string, Gadget> mGadgets;

        // ----- Public Variables ----- 
        public static readonly Gadget Bandage = new Gadget("Bandage");
        public static readonly Gadget Binoculars = new Gadget("Binoculars");
        public static readonly Gadget RangeFinder = new Gadget("Range Finder");
        public static readonly Gadget RepairTool = new Gadget("Repair Tool");
        public static readonly Gadget C4 = new Gadget("C4");
        public static readonly Gadget Claymore = new Gadget("Claymore");
        public static readonly Gadget M320SmokeGrenadeLauncher = new Gadget("M320 Smoke Grenade Launcher");
        public static readonly Gadget SmallAmmoKit = new Gadget("Small Ammo Kit");
        public static readonly Gadget AntiPersonnelMine = new Gadget("Anti Personnel Mine");
        public static readonly Gadget AntiVehicleMine = new Gadget("Anti Vehicle Mine");
        public static readonly Gadget MedicKit = new Gadget("Medic Kit");
        public static readonly Gadget Rpg7HeatExplosive = new Gadget("Rpg7 Heat Explosive");
        public static readonly Gadget RiotShield = new Gadget("Riot Shield");
        public static readonly Gadget FragGrenade = new Gadget("Frag Grenade");
        public static readonly Gadget ImpactGrenade = new Gadget("Impact Grenade");
        public static readonly Gadget AntiVehicleGrenade = new Gadget("Anti Vehicle Grenade");
        public static readonly Gadget SmokeGrenadeBlue = new Gadget("Smoke Grenade Blue");
        public static readonly Gadget SmokeGrenadeGreen = new Gadget("Smoke Grenade Green");
        public static readonly Gadget SmokeGrenadeRed = new Gadget("Smoke Grenade Red");
        public static readonly Gadget SmokeGrenadeWhite = new Gadget("Smoke Grenade White");
        public static readonly Gadget Flare = new Gadget("Flare");
        public static readonly Gadget SledgeHammer = new Gadget("Sledge Hammer");
        public static readonly Gadget AdvancedBinoculars = new Gadget("Advanced Binoculars");
        public static readonly Gadget Mdx201 = new Gadget("Mdx 201");
        public static readonly Gadget BinoSoflam = new Gadget("Bino Soflam");
        public static readonly Gadget HeavyAmmoKit = new Gadget("Heavy Ammo Kit");
        public static readonly Gadget Rpg7Pgo7Tandem = new Gadget("Rpg7 Pgo7 Tandem");
        public static readonly Gadget Rpg7Pgo7HeatExplosive = new Gadget("Rpg7 Pgo7 Heat Explosive");
        public static readonly Gadget Rpg7Pgo7Fragmentation = new Gadget("Rpg7 Pgo7 Fragmentation");
        public static readonly Gadget Rpg7Fragmentation = new Gadget("Rpg7 Fragmentation");
        public static readonly Gadget GrapplingHook = new Gadget("Grappling Hook");
        public static readonly Gadget AirDrone = new Gadget("Air Drone");
        public static readonly Gadget Flashbang = new Gadget("Flashbang");
        public static readonly Gadget Pickaxe = new Gadget("Pickaxe");
        public static readonly Gadget SuicideC4 = new Gadget("SuicideC4");
        public static readonly Gadget SledgeHammerSkinA = new Gadget("Sledge Hammer SkinA");
        public static readonly Gadget SledgeHammerSkinB = new Gadget("Sledge Hammer SkinB");
        public static readonly Gadget SledgeHammerSkinC = new Gadget("Sledge Hammer SkinC");
        public static readonly Gadget PickaxeIronPickaxe = new Gadget("Pickaxe IronPickaxe");

        // ----- Public Calls ----- 
        public static bool TryFind(string name, out Gadget item)
        {
            return mGadgets.TryGetValue(name, out item);
        }

        // ----- Init ----- 
        static Gadgets()
        {
            var members = typeof(Gadgets).GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            mGadgets = new Dictionary<string, Gadget>(members.Length);
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = ((FieldInfo)memberInfo);
                    if (field.FieldType == typeof(Gadget))
                    {
                        var gad = (Gadget)field.GetValue(null);
                        mGadgets.Add(gad.Name, gad);
                    }
                }
            }
        }
    }
}
