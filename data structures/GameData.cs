using UnityEngine;

[System.Serializable]
public record GameData {
    // TODO: static save, load method
    public GameState state;
    public PlayerState playerState;
    public LevelState levelState;

    // UI state:
    public int overlayIndex;

    public static GameData TestInitialData() {
        LevelTemplate levelTemplate = LevelTemplate.LoadAsInstance("test");
        PlayerState playerState = PlayerState.DefaultState();
        return new GameData() {
            state = GameState.none,
            playerState = playerState,
            levelState = LevelState.Instantiate(levelTemplate),
            overlayIndex = 0
        };
    }
}
