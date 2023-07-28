namespace BattleBitAPI.Networking
{
    public enum NetworkCommuncation : byte
    {
        None = 0,
        Hail = 1,
        Accepted = 2,
        Denied = 3,

        ExecuteCommand = 10,
        SendPlayerStats = 11,

        PlayerConnected = 50,
        PlayerDisconnected = 51,
        OnPlayerTypedMessage = 52,
        OnPlayerKilledAnotherPlayer = 53,
        GetPlayerStats = 54,
        SavePlayerStats = 55,

    }
}
