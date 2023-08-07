using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerTypedMessageEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player who typed the message.
        /// </summary>
        public TPlayer Player { get; init; }

        /// <summary>
        /// The channel the message was sent in.
        /// </summary>
        public ChatChannel ChatChannel { get; init; }

        /// <summary>
        /// The message.
        /// </summary>
        public string Message { get; init; }

        internal PlayerTypedMessageEventArgs(TPlayer player, ChatChannel chatChannel, string message)
        {
            Player = player;
            ChatChannel = chatChannel;
            Message = message;
        }
    }
}