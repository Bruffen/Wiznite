namespace Common
{
    public enum GameState
    {
        LobbyDisconnected,
		LobbyDisconnecting,
        LobbyCreation,
        LobbyConnecting,
        LobbyUnready,
        LobbySync,
        LobbyReady,
        LobbiesRequest,
        GameStarted,
        GameDisconnected,
        GameConnecting,
        GameConnected,
        GameSync,
        GameEnd
    }
}
