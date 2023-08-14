using System.Reflection;

namespace BattleBitAPI.Common;

public static class Weapons
{
    // ----- Private Variables ----- 
    private static readonly Dictionary<string, Weapon> mWeapons;

    // ----- Public Variables ----- 
    public static readonly Weapon ACR = new("ACR", WeaponType.Rifle);
    public static readonly Weapon AK15 = new("AK15", WeaponType.Rifle);
    public static readonly Weapon AK74 = new("AK74", WeaponType.Rifle);
    public static readonly Weapon G36C = new("G36C", WeaponType.Rifle);
    public static readonly Weapon HoneyBadger = new("Honey Badger", WeaponType.PersonalDefenseWeapon_PDW);
    public static readonly Weapon KrissVector = new("Kriss Vector", WeaponType.SubmachineGun_SMG);
    public static readonly Weapon L86A1 = new("L86A1", WeaponType.LightSupportGun_LSG);
    public static readonly Weapon L96 = new("L96", WeaponType.SniperRifle);
    public static readonly Weapon M4A1 = new("M4A1", WeaponType.Rifle);
    public static readonly Weapon M9 = new("M9", WeaponType.Pistol);
    public static readonly Weapon M110 = new("M110", WeaponType.DMR);
    public static readonly Weapon M249 = new("M249", WeaponType.LightMachineGun_LMG);
    public static readonly Weapon MK14EBR = new("MK14 EBR", WeaponType.DMR);
    public static readonly Weapon MK20 = new("MK20", WeaponType.DMR);
    public static readonly Weapon MP7 = new("MP7", WeaponType.SubmachineGun_SMG);
    public static readonly Weapon PP2000 = new("PP2000", WeaponType.SubmachineGun_SMG);
    public static readonly Weapon SCARH = new("SCAR-H", WeaponType.Rifle);
    public static readonly Weapon SSG69 = new("SSG 69", WeaponType.SniperRifle);
    public static readonly Weapon SV98 = new("SV-98", WeaponType.SniperRifle);
    public static readonly Weapon UMP45 = new("UMP-45", WeaponType.SubmachineGun_SMG);
    public static readonly Weapon Unica = new("Unica", WeaponType.HeavyPistol);
    public static readonly Weapon USP = new("USP", WeaponType.Pistol);
    public static readonly Weapon AsVal = new("As Val", WeaponType.Carbine);
    public static readonly Weapon AUGA3 = new("AUG A3", WeaponType.Rifle);
    public static readonly Weapon DesertEagle = new("Desert Eagle", WeaponType.HeavyPistol);
    public static readonly Weapon FAL = new("FAL", WeaponType.Rifle);
    public static readonly Weapon Glock18 = new("Glock 18", WeaponType.AutoPistol);
    public static readonly Weapon M200 = new("M200", WeaponType.SniperRifle);
    public static readonly Weapon MP443 = new("MP 443", WeaponType.Pistol);
    public static readonly Weapon FAMAS = new("FAMAS", WeaponType.Rifle);
    public static readonly Weapon MP5 = new("MP5", WeaponType.SubmachineGun_SMG);
    public static readonly Weapon P90 = new("P90", WeaponType.PersonalDefenseWeapon_PDW);
    public static readonly Weapon MSR = new("MSR", WeaponType.SniperRifle);
    public static readonly Weapon PP19 = new("PP19", WeaponType.SubmachineGun_SMG);
    public static readonly Weapon SVD = new("SVD", WeaponType.DMR);
    public static readonly Weapon Rem700 = new("Rem700", WeaponType.SniperRifle);
    public static readonly Weapon SG550 = new("SG550", WeaponType.Rifle);
    public static readonly Weapon Groza = new("Groza", WeaponType.PersonalDefenseWeapon_PDW);
    public static readonly Weapon HK419 = new("HK419", WeaponType.Rifle);
    public static readonly Weapon ScorpionEVO = new("ScorpionEVO", WeaponType.Carbine);
    public static readonly Weapon Rsh12 = new("Rsh12", WeaponType.HeavyPistol);
    public static readonly Weapon MG36 = new("MG36", WeaponType.LightSupportGun_LSG);
    public static readonly Weapon AK5C = new("AK5C", WeaponType.Rifle);
    public static readonly Weapon Ultimax100 = new("Ultimax100", WeaponType.LightMachineGun_LMG);

    // ----- Init ----- 
    static Weapons()
    {
        var members = typeof(Weapons).GetMembers(BindingFlags.Public | BindingFlags.Static);
        mWeapons = new Dictionary<string, Weapon>(members.Length);
        foreach (var memberInfo in members)
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)memberInfo;
                if (field.FieldType == typeof(Weapon))
                {
                    var wep = (Weapon)field.GetValue(null);
                    mWeapons.Add(wep.Name, wep);
                }
            }
    }

    // ----- Public Calls ----- 
    public static bool TryFind(string name, out Weapon item)
    {
        return mWeapons.TryGetValue(name, out item);
    }
}