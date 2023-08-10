using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Diagnostics;
using System.Net;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer>();
        listener.Start(29294);

        Thread.Sleep(-1);
    }

}
class MyPlayer : Player
{

}