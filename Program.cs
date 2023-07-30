using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitAPI.Storage;
using System.Numerics;

class Program
{

    static DiskStorage playerStats;
    static void Main(string[] args)
    {
        playerStats = new DiskStorage("Players\\");
        var listener = new ServerListener<MyPlayer>();
        listener.OnGetPlayerStats += OnGetPlayerStats;
        listener.OnPlayerSpawning += OnPlayerSpawning;
        listener.Start(29294);//Port

        Thread.Sleep(-1);
    }



    private static async Task<PlayerSpawnRequest> OnPlayerSpawning(MyPlayer player, PlayerSpawnRequest request)
    {
        if (request.Loadout.PrimaryWeapon.Tool == Weapons.M4A1)
        {
            //Don't allow M4A1
            request.Loadout.PrimaryWeapon.Tool = null;
        }
        else if (request.Loadout.PrimaryWeapon.Tool.WeaponType == WeaponType.SniperRifle)
        {
            //Force 6x if weapon is sniper.
            request.Loadout.PrimaryWeapon.MainSight = Attachments._6xScope;
        }

        //Override pistol with deagle
        request.Loadout.SecondaryWeapon.Tool = Weapons.DesertEagle;

        //Force everyone to use RPG
        request.Loadout.LightGadget = Gadgets.Rpg7HeatExplosive;

        //Don't allow C4s
        if (request.Loadout.HeavyGadget == Gadgets.C4)
            request.Loadout.HeavyGadget = null;

        //Spawn player 2 meter above than the original position.
        request.SpawnPosition.Y += 2f;

        //Remove spawn protection
        request.SpawnProtection = 0f;

        //Remove chest armor
        request.Wearings.Chest = null;

        //Give extra 10 more magazines on primary
        request.Loadout.PrimaryExtraMagazines += 10;

        //Give extra 5 more throwables 
        request.Loadout.ThrowableExtra += 5;

        return request;
    }




    private async static Task<PlayerStats> OnGetPlayerStats(ulong steamID, PlayerStats officialStats)
    {
        officialStats.Progress.Rank = 200;
        return officialStats;
    }
}
class MyPlayer : Player
{
    public int Cash;
    public bool InJail = false;
}
