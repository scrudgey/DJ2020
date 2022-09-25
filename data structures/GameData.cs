using UnityEngine;

[System.Serializable]
public class GameData {
    // TODO: static save, load method
    public GameState state;
    public PlayerState playerData;
    public LevelState levelData;

    // UI state:
    public int overlayIndex;

    public static GameData TestInitialData() {
        // LevelTemplate levelTemplate = ScriptableObject.Instantiate(Resources.Load("data/levels/test/levelTemplate") as LevelTemplate);
        LevelTemplate levelTemplate = LevelTemplate.Load("test");

        return new GameData() {
            state = GameState.none,
            playerData = PlayerState.DefaultState(),
            // levelData = LevelData.LoadLevelData("test"),
            levelData = LevelState.Instantiate(levelTemplate),
            overlayIndex = 0
        };
    }
}
