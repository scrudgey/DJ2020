[System.Serializable]
public class GameData {
    // TODO static save, load method
    public GameState state;
    public PlayerData playerData;
    public LevelData levelData;


    // UI state:
    public int overlayIndex;

    public static GameData TestInitialData() {
        return new GameData() {
            state = GameState.none,
            playerData = PlayerData.DefaultGameData(),
            levelData = LevelData.LoadLevelData("test")
        };
    }
}
