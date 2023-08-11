namespace BattleBitAPI.Common
{
    public enum GameState : byte
    {
        WaitingForPlayers = 0,
        CountingDown = 1,
        Playing = 2,
        EndingGame = 3
    }
}
