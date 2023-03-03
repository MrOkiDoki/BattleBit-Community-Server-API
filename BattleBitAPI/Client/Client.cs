using BattleBitAPI.Common.Enums;
using BattleBitAPI.Common.Serialization;
using BattleBitAPI.Networking;
using System;
using System.Net;
using System.Net.Sockets;

namespace BattleBitAPI.Client
{
    // This class was created mainly for Unity Engine, for this reason, Task async was not implemented.
    public class Client
    {
        // ---- Public Variables ---- 
        public bool IsConnected { get; private set; }
        public int Port { get; private set; }
        public bool IsPasswordProtected { get; private set; }
        public string ServerName { get; private set; }
        public string Gamemode { get; private set; }
        public string Map { get; private set; }
        public MapSize MapSize { get; private set; }
        public MapDayNight DayNight { get; private set; }
        public int CurrentPlayers { get; private set; }
        public int InQueuePlayers { get; private set; }
        public int MaxPlayers { get; private set; }
        public string LoadingScreenText { get; private set; }
        public string ServerRulesText { get; private set; }

        // ---- Private Variables ---- 
        private TcpClient mClient;
        private string mDestination;
        private int mPort;
        private bool mIsConnectingFlag;

        // ---- Construction ---- 
        public Client(string destination, int port)
        {
            this.mDestination = destination;
            this.mPort = port;
        }

        // ---- Main Tick ---- 
        public void Tick()
        {
            //Are we connecting?
            if (mIsConnectingFlag)
                return;

            //Have we connected?
            if (!this.IsConnected)
            {
                //Attempt to connect to server async.
                this.mIsConnectingFlag = true;

                var state = mClient.BeginConnect(mDestination, mPort, (x) =>
                {
                    this.mIsConnectingFlag = false;

                    //Did we connect?
                    try { mClient.EndConnect(x); }
                    catch { return; }

                    using (var hail = BattleBitAPI.Common.Serialization.Stream.Get())
                    {
                        hail.Write((byte)NetworkCommuncation.Hail);
                        hail.Write((ushort)this.Port);
                        hail.Write(this.IsPasswordProtected);
                        hail.Write(this.ServerName);
                        hail.Write(this.Gamemode);
                        hail.Write(this.Map);
                        hail.Write((byte)this.MapSize);
                        hail.Write((byte)this.DayNight);
                        hail.Write((byte)this.CurrentPlayers);
                        hail.Write((byte)this.InQueuePlayers);
                        hail.Write((byte)this.MaxPlayers);
                        hail.Write(this.LoadingScreenText);
                        hail.Write(this.ServerRulesText);

                        //Send our hail package.
                        mClient.GetStream().Write(hail.Buffer, 0, hail.WritePosition);
                    }

                }, null);

                return;
            }
        }

        // ---- Private ---- 



    }
}
