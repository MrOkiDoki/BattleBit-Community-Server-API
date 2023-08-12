using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Starting API");
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.Start(55669);
        Thread.Sleep(-1);
    }
}

public class MyPlayer : Player<MyPlayer>
{
    private readonly List<Weapon> mGunGame = new()
    {
        Weapons.Glock18,
        Weapons.Groza,
        Weapons.ACR,
        Weapons.AK15,
        Weapons.AK74,
        Weapons.G36C,
        Weapons.HoneyBadger,
        Weapons.KrissVector,
        Weapons.L86A1,
        Weapons.L96,
        Weapons.M4A1,
        Weapons.M9,
        Weapons.M110,
        Weapons.M249,
        Weapons.MK14EBR,
        Weapons.MK20,
        Weapons.MP7,
        Weapons.PP2000,
        Weapons.SCARH,
        Weapons.SSG69
    };

    public int Level;

    public void UpdateWeapon()
    {
        var w = new WeaponItem
        {
            ToolName = mGunGame[Level].Name,
            MainSight = Attachments.RedDot
        };
        SetPrimaryWeapon(w, 10, true);
    }
}

internal class MyGameServer : GameServer<MyPlayer>
{
    public override async Task OnConnected()
    {
        await Console.Out.WriteLineAsync(GameIP + " Connected");
    }


    public override Task OnPlayerSpawned(MyPlayer player)
    {
        Task.Run(() =>
            {
                player.UpdateWeapon();
                player.SetRunningSpeedMultiplier(1.25f);
                player.SetFallDamageMultiplier(0f);
                player.SetJumpMultiplier(1.5f);
            }
        );
        return base.OnPlayerSpawned(player);
    }


    public override async Task OnDisconnected()
    {
        await Console.Out.WriteLineAsync(GameIP + " Disconnected");
    }

    public override async Task<bool> OnAPlayerKilledAnotherPlayer(OnPlayerKillArguments<MyPlayer> onPlayerKillArguments)
    {
        var killer = onPlayerKillArguments.Killer;
        var victim = onPlayerKillArguments.Victim;
        killer.Level++;
        killer.UpdateWeapon();
        return true;
    }
}