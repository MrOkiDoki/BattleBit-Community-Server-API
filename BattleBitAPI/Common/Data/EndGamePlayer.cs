namespace BattleBitAPI.Common
{
    public struct EndGamePlayer<TPlayer> : IComparable<EndGamePlayer<TPlayer>> where TPlayer : Player<TPlayer>
    {
        public Player<TPlayer> Player;
        public int Score;

        public EndGamePlayer(Player<TPlayer> player, int score)
        {
            this.Player = player;
            this.Score = score;
        }
        public int CompareTo(EndGamePlayer<TPlayer> other)
        {
            return other.Score.CompareTo(this.Score);
        }
    }
}
