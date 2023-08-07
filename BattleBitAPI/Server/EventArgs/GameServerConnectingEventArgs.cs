using System.Net;

namespace BattleBitAPI.Server.EventArgs
{
    public class GameServerConnectingEventArgs
    {
        /// <summary>
        /// IP of incoming connection
        /// </summary>
        public IPAddress IPAddress { get; init; }

        /// <summary>
        /// Whether to allow the connection or not.
        /// </summary>
        public bool Allow { get; set; }

        internal GameServerConnectingEventArgs(IPAddress ipAddress)
        {
            IPAddress = ipAddress;
            Allow = true;
        }
    }
}