public class GameData {
    // TODO static save, load method
    public GameState state;
    public PlayerData playerData;
    public static GameData TestInitialData() {
        return new GameData() {
            state = GameState.none,
            playerData = PlayerData.DefaultGameData()
        };
    }
    public LevelData levelData;
}
