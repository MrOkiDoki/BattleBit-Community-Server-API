using Microsoft.VisualBasic;
using System.Reflection;

namespace BattleBitAPI.Common
{
    public static class Weapons
    {
        // ----- Private Variables ----- 
        private static Dictionary<string, Weapon> mWeapons;

        // ----- Public Variables ----- 
        public readonly static Weapon ACR = new Weapon("ACR", WeaponType.Rifle);
        public readonly static Weapon AK15 = new Weapon("AK15", WeaponType.Rifle);
        public readonly static Weapon AK74 = new Weapon("AK74", WeaponType.Rifle);
        public readonly static Weapon G36C = new Weapon("G36C", WeaponType.Rifle);
        public readonly static Weapon HoneyBadger = new Weapon("Honey Badger", WeaponType.PersonalDefenseWeapon_PDW);
        public readonly static Weapon KrissVector = new Weapon("Kriss Vector", WeaponType.SubmachineGun_SMG);
        public readonly static Weapon L86A1 = new Weapon("L86A1", WeaponType.LightSupportGun_LSG);
        public readonly static Weapon L96 = new Weapon("L96", WeaponType.SniperRifle);
        public readonly static Weapon M4A1 = new Weapon("M4A1", WeaponType.Rifle);
        public readonly static Weapon M9 = new Weapon("M9", WeaponType.Pistol);
        public readonly static Weapon M110 = new Weapon("M110", WeaponType.DMR);
        public readonly static Weapon M249 = new Weapon("M249", WeaponType.LightMachineGun_LMG);
        public readonly static Weapon MK14EBR = new Weapon("MK14 EBR", WeaponType.DMR);
        public readonly static Weapon MK20 = new Weapon("MK20", WeaponType.DMR);
        public readonly static Weapon MP7 = new Weapon("MP7", WeaponType.SubmachineGun_SMG);
        public readonly static Weapon PP2000 = new Weapon("PP2000", WeaponType.SubmachineGun_SMG);
        public readonly static Weapon SCARH = new Weapon("SCAR-H", WeaponType.Rifle);
        public readonly static Weapon SSG69 = new Weapon("SSG 69", WeaponType.SniperRifle);
        public readonly static Weapon SV98 = new Weapon("SV-98", WeaponType.SniperRifle);
        public readonly static Weapon UMP45 = new Weapon("UMP-45", WeaponType.SubmachineGun_SMG);
        public readonly static Weapon Unica = new Weapon("Unica", WeaponType.HeavyPistol);
        public readonly static Weapon USP = new Weapon("USP", WeaponType.Pistol);
        public readonly static Weapon AsVal = new Weapon("As Val", WeaponType.Carbine);
        public readonly static Weapon AUGA3 = new Weapon("AUG A3", WeaponType.Rifle);
        public readonly static Weapon DesertEagle = new Weapon("Desert Eagle", WeaponType.HeavyPistol);
        public readonly static Weapon FAL = new Weapon("FAL", WeaponType.Rifle);
        public readonly static Weapon Glock18 = new Weapon("Glock 18", WeaponType.AutoPistol);
        public readonly static Weapon M200 = new Weapon("M200", WeaponType.SniperRifle);
        public readonly static Weapon MP443 = new Weapon("MP 443", WeaponType.Pistol);
        public readonly static Weapon FAMAS = new Weapon("FAMAS", WeaponType.Rifle);
        public readonly static Weapon MP5 = new Weapon("MP5", WeaponType.SubmachineGun_SMG);
        public readonly static Weapon P90 = new Weapon("P90", WeaponType.PersonalDefenseWeapon_PDW);
        public readonly static Weapon MSR = new Weapon("MSR", WeaponType.SniperRifle);
        public readonly static Weapon PP19 = new Weapon("PP19", WeaponType.SubmachineGun_SMG);
        public readonly static Weapon SVD = new Weapon("SVD", WeaponType.DMR);
        public readonly static Weapon Rem700 = new Weapon("Rem700", WeaponType.SniperRifle);
        public readonly static Weapon SG550 = new Weapon("SG550", WeaponType.Rifle);
        public readonly static Weapon Groza = new Weapon("Groza", WeaponType.PersonalDefenseWeapon_PDW);
        public readonly static Weapon HK419 = new Weapon("HK419", WeaponType.Rifle);
        public readonly static Weapon ScorpionEVO = new Weapon("ScorpionEVO", WeaponType.Carbine);
        public readonly static Weapon Rsh12 = new Weapon("Rsh12", WeaponType.HeavyPistol);
        public readonly static Weapon MG36 = new Weapon("MG36", WeaponType.LightSupportGun_LSG);
        public readonly static Weapon AK5C = new Weapon("AK5C", WeaponType.Rifle);
        public readonly static Weapon Ultimax100 = new Weapon("Ultimax100", WeaponType.LightMachineGun_LMG);

        // ----- Public Calls ----- 
        public static bool TryFind(string name, out Weapon item)
        {
            return mWeapons.TryGetValue(name, out item);
        }

        // ----- Init ----- 
        static Weapons()
        {
            var members = typeof(Weapons).GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            mWeapons = new Dictionary<string, Weapon>(members.Length);
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = ((FieldInfo)memberInfo);
                    if (field.FieldType == typeof(Weapon))
                    {
                        var wep = (Weapon)field.GetValue(null);
                        mWeapons.Add(wep.Name, wep);
                    }
                }
            }
        }
    }
}
