public class GameData {
    // TODO static save, load method
    public GameState state;
    public PlayerData playerState;
    public static GameData TestInitialData() {
        return new GameData() {
            state = GameState.none,
            playerState = PlayerData.TestInitialData()
        };
    }
}
