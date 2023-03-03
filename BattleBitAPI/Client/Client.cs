using BattleBitAPI.Common.Enums;
using BattleBitAPI.Common.Serialization;
using BattleBitAPI.Networking;
using CommunityServerAPI.BattleBitAPI;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace BattleBitAPI.Client
{
    // This class was created mainly for Unity Engine, for this reason, Task async was not implemented.
    public class Client
    {
        // ---- Public Variables ---- 
        public bool IsConnected { get; set; }
        public int GamePort { get; set; }
        public bool IsPasswordProtected { get; set; }
        public string ServerName { get; set; }
        public string Gamemode { get; set; }
        public string Map { get; set; }
        public MapSize MapSize { get; set; }
        public MapDayNight DayNight { get; set; }
        public int CurrentPlayers { get; set; }
        public int InQueuePlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string LoadingScreenText { get; set; }
        public string ServerRulesText { get; set; }

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

                //Dispose old client if exist.
                if (this.mClient != null)
                {
                    try { this.mClient.Close(); } catch { }
                    try { this.mClient.Dispose(); } catch { }
                    this.mClient = null;
                }

                //Create new client
                this.mClient = new TcpClient();
                this.mClient.SendBufferSize = Const.MaxNetworkPackageSize;
                this.mClient.ReceiveBufferSize = Const.MaxNetworkPackageSize;

                //Attempt to connect.
                var state = mClient.BeginConnect(mDestination, mPort, (x) =>
                {
                    this.mIsConnectingFlag = false;

                    try
                    {
                        //Did we connect?
                        mClient.EndConnect(x);

                        var networkStream = mClient.GetStream();

                        //Prepare our hail package and send it.
                        using (var hail = BattleBitAPI.Common.Serialization.Stream.Get())
                        {
                            hail.Write((byte)NetworkCommuncation.Hail);
                            hail.Write((ushort)this.GamePort);
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
                            networkStream.Write(hail.Buffer, 0, hail.WritePosition);
                            networkStream.Flush();
                        }

                        //Sadly can not use Task Async here, Unity isn't great with tasks.
                        var watch = Stopwatch.StartNew();

                        //Read the first byte.
                        NetworkCommuncation response = NetworkCommuncation.None;
                        while (watch.ElapsedMilliseconds < Const.HailConnectTimeout)
                        {
                            if (mClient.Available > 0)
                            {
                                var data = networkStream.ReadByte();
                                if (data >= 0)
                                {
                                    response = (NetworkCommuncation)data;
                                    break;
                                }
                            }
                            Thread.Sleep(1);
                        }

                        //Were we accepted.
                        if (response == NetworkCommuncation.Accepted)
                        {
                            //We are accepted.
                            this.IsConnected = true;

                            mOnConnectedToServer();
                        }
                        else
                        {
                            //Did we at least got a response?
                            if (response == NetworkCommuncation.None)
                                throw new Exception("Server did not respond to your connect request.");

                            //Try to read our deny reason.
                            if (response == NetworkCommuncation.Denied && mClient.Available > 0)
                            {
                                string errorString = null;

                                using (var readStream = BattleBitAPI.Common.Serialization.Stream.Get())
                                {
                                    readStream.WritePosition = networkStream.Read(readStream.Buffer, 0, mClient.Available);
                                    if (!readStream.TryReadString(out errorString))
                                        errorString = null;
                                }

                                if (errorString != null)
                                    throw new Exception(errorString);
                            }

                            throw new Exception("Server denied our connect request with an unknown reason.");
                        }
                    }
                    catch (Exception e)
                    {
                        mLogError("Unable to connect to API server: " + e.Message);
                        return;
                    }

                }, null);

                //We haven't connected yet.
                return;
            }

            //We are connected at this point.

        }

        // ---- Callbacks ---- 
        private void mOnConnectedToServer()
        {

        }
        private void mOnDisconnectedFromServer(string reason)
        {
        }


        // ---- Private ---- 
        private void mLogError(string str)
        {

        }
        private void mCloseConnection(string reason)
        {
            if (this.IsConnected)
            {
                this.IsConnected = false;

                //Dispose old client if exist.
                if (this.mClient != null)
                {
                    try { this.mClient.Close(); } catch { }
                    try { this.mClient.Dispose(); } catch { }
                    this.mClient = null;
                }

                mOnDisconnectedFromServer(reason);
            }
        }
    }
}
