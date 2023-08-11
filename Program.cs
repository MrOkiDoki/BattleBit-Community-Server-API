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
    public bool IsZombie;
}
class MyGameServer : GameServer<MyPlayer>
{

    public override async Task OnRoundStarted()
    {
    }
    public override async Task OnRoundEnded()
    {
    }

    public override async Task OnPlayerConnected(MyPlayer player)
    {
        bool anyZombiePlayer = false;
        foreach (var item in AllPlayers)
        {
            if (item.IsZombie)
            {
                anyZombiePlayer = true;
                break;
            }
        }

        if (!anyZombiePlayer)
        {
            player.IsZombie = true;
            player.Message("You are the zombie.");
            player.Kill();
        }
    }

    public override async Task OnAPlayerKilledAnotherPlayer(OnPlayerKillArguments<MyPlayer> args)
    {
        if (args.Victim.IsZombie)
        {
            args.Victim.IsZombie = false;
            args.Victim.Message("You are no longer zombie");

            AnnounceShort("Choosing new zombie in 5");
            await Task.Delay(1000);
            AnnounceShort("Choosing new zombie in 4");
            await Task.Delay(1000);
            AnnounceShort("Choosing new zombie in 3");
            await Task.Delay(1000);
            AnnounceShort("Choosing new zombie in 2");
            await Task.Delay(1000);
            AnnounceShort("Choosing new zombie in 1");
            await Task.Delay(1000);

            args.Killer.IsZombie = true;
            args.Killer.SetHeavyGadget(Gadgets.SledgeHammer.ToString(), 0, true);

            var position = args.Killer.GetPosition();
        }
    }


    public override async Task<OnPlayerSpawnArguments> OnPlayerSpawning(MyPlayer player, OnPlayerSpawnArguments request)
    {
        if (player.IsZombie)
        {
            request.Loadout.PrimaryWeapon = default;
            request.Loadout.SecondaryWeapon = default;
            request.Loadout.LightGadget = null;
            request.Loadout.HeavyGadget = Gadgets.SledgeHammer;
            request.Loadout.Throwable = null;
        }

        return request;
    }
    public override async Task OnPlayerSpawned(MyPlayer player)
    {
        if(player.IsZombie)
        {
            player.SetRunningSpeedMultiplier(2f);
            player.SetJumpMultiplier(2f);
            player.SetFallDamageMultiplier(0f);
            player.SetReceiveDamageMultiplier(0.1f);
            player.SetGiveDamageMultiplier(4f);
        }
    }



    public override async Task OnConnected()
    {
        await Console.Out.WriteLineAsync("Current state: " + RoundSettings.State);

    }
    public override async Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        await Console.Out.WriteLineAsync("State changed to -> " + newState);
    }
}
