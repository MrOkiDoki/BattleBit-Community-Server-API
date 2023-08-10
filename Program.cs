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

        listener.OnPlayerTypedMessage += OnPlayerTypedMessage;

        Thread.Sleep(-1);
    }

    private static async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel ch, string msg)
    {
        if (msg == "nword")
        {
            player.NumberOfNWord++;
            if (player.NumberOfNWord > 4)
            {
                player.Kick("N word is not accepted!");
            }
            else
            {
                player.Message("Do not type nword, this is your " + player.NumberOfNWord + "th warning");
            }
            return false;
        }
        return true;
    }
}
class MyPlayer : Player
{
    public int NumberOfNWord;
}