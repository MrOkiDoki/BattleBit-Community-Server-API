using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    /// <remarks>
    /// Player: The player that typed the message <br/>
    /// ChatChannel: The channel the message was sent <br/>
    /// string - Message: The message<br/>
    /// </remarks>
    public class PlayerTypedMessageEventArgs<TPlayer> where TPlayer : Player
    {
        public TPlayer Player { get; init; }
        public ChatChannel ChatChannel { get; init; }
        public string Message { get; init; }

        internal PlayerTypedMessageEventArgs(TPlayer player, ChatChannel chatChannel, string message)
        {
            Player = player;
            ChatChannel = chatChannel;
            Message = message;
        }
    }
}