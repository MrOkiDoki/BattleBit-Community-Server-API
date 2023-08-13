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
    private readonly List<Weapon> gunGame = new()
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

    public async void UpdateWeapon()
    {
        await Task.Run(() =>
        {
            if (Level < gunGame.Count)
            {
                var w = new WeaponItem
                {
                    ToolName = gunGame[Level].Name,
                    MainSight = Attachments.RedDot
                };
                SetPrimaryWeapon(w, 10, true);
            }
        });
    }

    public int GetGameLenght()
    {
        return gunGame.Count;
    }
}

internal class MyGameServer : GameServer<MyPlayer>
{
    // Gun Game
    public override async Task OnPlayerSpawned(MyPlayer player)
    {
        await Task.Run(() =>
        {
            player.UpdateWeapon();
            player.SetRunningSpeedMultiplier(1.25f);
            player.SetFallDamageMultiplier(0f);
            player.SetJumpMultiplier(1.5f);
        });
    }

    public override async Task<bool> OnAPlayerKilledAnotherPlayer(OnPlayerKillArguments<MyPlayer> onPlayerKillArguments)
    {
        await Task.Run(() =>
        {
            var killer = onPlayerKillArguments.Killer;
            var victim = onPlayerKillArguments.Victim;
            killer.Level++;
            if (killer.Level == killer.GetGameLenght()) AnnounceShort($"{killer.Name} only needs 1 more Kill");
            if (killer.Level > killer.GetGameLenght())
            {
                AnnounceShort($"{killer.Name} won the Game");
                ForceEndGame();
            }

            if (onPlayerKillArguments.KillerTool == "Sledge Hammer" && victim.Level != 0) victim.Level--;
            killer.UpdateWeapon();
        });
        return true;
    }

    public override Task OnRoundEnded()
    {
        foreach (var player in AllPlayers) player.Level = 0;
        return base.OnRoundEnded();
    }

    public override Task OnConnected()
    {
        Console.WriteLine("Server connected");
        return base.OnConnected();
    }

    public override Task OnDisconnected()
    {
        Console.WriteLine("Server disconnected");
        return base.OnDisconnected();
    }

    public override Task OnPlayerConnected(MyPlayer player)
    {
        Console.WriteLine($"{player.Name} connected");
        return base.OnPlayerConnected(player);
    }

    public override Task OnPlayerDisconnected(MyPlayer player)
    {
        Console.WriteLine($"{player.Name} disconnected");
        return base.OnPlayerDisconnected(player);
    }


    public override Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        if (msg.StartsWith("start")) ForceStartGame();

        return base.OnPlayerTypedMessage(player, channel, msg);
    }
}