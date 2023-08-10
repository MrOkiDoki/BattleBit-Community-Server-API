using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer>();
        listener.Start(29294);

        listener.OnAPlayerKilledAnotherPlayer += OnAPlayerKilledAnotherPlayer;

        Thread.Sleep(-1);
    }

    private static async Task OnAPlayerKilledAnotherPlayer(OnPlayerKillArguments<MyPlayer> arg)
    {
        await Console.Out.WriteLineAsync(arg.Killer + " killed " + arg.Victim + " with " + arg.KillerTool + " (" + arg.BodyPart + ")");
    }
}
class MyPlayer : Player
{
    public int NumberOfNWord;
}