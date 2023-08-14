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
        SpawnPlayer = 12,
        SetNewRoomSettings = 13,
        RespondPlayerMessage = 14,
        SetNewRoundState = 15,
        SetPlayerWeapon = 16,
        SetPlayerGadget = 17,

        PlayerConnected = 50,
        PlayerDisconnected = 51,
        OnPlayerTypedMessage = 52,
        OnAPlayerDownedAnotherPlayer = 53,
        OnPlayerJoining = 54,
        SavePlayerStats = 55,
        OnPlayerAskingToChangeRole = 56,
        OnPlayerChangedRole = 57,
        OnPlayerJoinedASquad = 58,
        OnPlayerLeftSquad = 59,
        OnPlayerChangedTeam = 60,
        OnPlayerRequestingToSpawn = 61,
        OnPlayerReport = 62,
        OnPlayerSpawn = 63,
        OnPlayerDie = 64,
        NotifyNewMapRotation = 65,
        NotifyNewGamemodeRotation = 66,
        NotifyNewRoundState = 67,
        OnPlayerAskingToChangeTeam = 68,
        GameTick = 69,
        OnPlayerGivenUp = 70,
        OnPlayerRevivedAnother = 71,
    }
}
