[System.Serializable]
public class GameData {
    // TODO static save, load method
    public GameState state;
    public PlayerState playerData;
    public LevelData levelData;


    // UI state:
    public int overlayIndex;

    public static GameData TestInitialData() {
        return new GameData() {
            state = GameState.none,
            playerData = PlayerState.DefaultGameData(),
            // levelData = LevelData.LoadLevelData("test"),
            levelData = LevelData.Load("test"),
            overlayIndex = 0
        };
    }
}
