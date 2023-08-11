using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Threading.Channels;
using System.Xml;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(29294);

        Thread.Sleep(-1);
    }
}
class MyPlayer : Player<MyPlayer>
{
    public int NumberOfKills;
}
class MyGameServer : GameServer<MyPlayer>
{
    public WeaponItem[] WeaponList = new WeaponItem[]
    {
        new WeaponItem(){ Tool = Weapons.M4A1, MainSight = Attachments._6xScope},
        new WeaponItem(){ Tool = Weapons.SVD, MainSight = Attachments._6xScope},
        new WeaponItem(){ Tool = Weapons.SCARH, MainSight = Attachments._6xScope},
        new WeaponItem(){ Tool = Weapons.ScorpionEVO, MainSight = Attachments._6xScope},
        new WeaponItem(){ Tool = Weapons.M249, MainSight = Attachments._6xScope},
        new WeaponItem(){ Tool = Weapons.Groza, MainSight = Attachments._6xScope},
        new WeaponItem(){ Tool = Weapons.Glock18, MainSight = Attachments._6xScope},
    };

    public override async Task OnTick()
    {
        if (this.RoundSettings.State == GameState.WaitingForPlayers)
            ForceStartGame();

        await Task.Delay(1000);
    }

    public override async Task<OnPlayerSpawnArguments> OnPlayerSpawning(MyPlayer player, OnPlayerSpawnArguments request)
    {
        request.Loadout.PrimaryWeapon = WeaponList[player.NumberOfKills];
        return request;
    }

    public override async Task OnAPlayerKilledAnotherPlayer(OnPlayerKillArguments<MyPlayer> args)
    {
        args.Killer.NumberOfKills++;
        args.Killer.SetPrimaryWeapon(WeaponList[args.Killer.NumberOfKills], 0);
    }
}
