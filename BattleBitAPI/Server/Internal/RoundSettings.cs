using BattleBitAPI.Common;

namespace BattleBitAPI.Server;

public class RoundSettings<TPlayer> where TPlayer : Player<TPlayer>
{
    private readonly GameServer<TPlayer>.Internal mResources;

    public RoundSettings(GameServer<TPlayer>.Internal resources)
    {
        mResources = resources;
    }

    public GameState State => mResources._RoundSettings.State;

    public double TeamATickets
    {
        get => mResources._RoundSettings.TeamATickets;
        set
        {
            mResources._RoundSettings.TeamATickets = value;
            mResources.IsDirtyRoundSettings = true;
        }
    }

    public double TeamBTickets
    {
        get => mResources._RoundSettings.TeamBTickets;
        set
        {
            mResources._RoundSettings.TeamBTickets = value;
            mResources.IsDirtyRoundSettings = true;
        }
    }

    public double MaxTickets
    {
        get => mResources._RoundSettings.MaxTickets;
        set
        {
            mResources._RoundSettings.MaxTickets = value;
            mResources.IsDirtyRoundSettings = true;
        }
    }

    public int PlayersToStart
    {
        get => mResources._RoundSettings.PlayersToStart;
        set
        {
            mResources._RoundSettings.PlayersToStart = value;
            mResources.IsDirtyRoundSettings = true;
        }
    }

    public int SecondsLeft
    {
        get => mResources._RoundSettings.SecondsLeft;
        set
        {
            mResources._RoundSettings.SecondsLeft = value;
            mResources.IsDirtyRoundSettings = true;
        }
    }

    public void Reset()
    {
    }
}