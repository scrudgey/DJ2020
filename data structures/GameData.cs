using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public record GameData {
    // TODO: static save, load method
    public GameState state;
    public PlayerState playerState;
    public LevelState levelState;
    // UI state: ???
    public List<string> unlockedLevels;
    public int overlayIndex;

    public static GameData TestInitialData() {
        LevelTemplate levelTemplate = LevelTemplate.LoadAsInstance("test");
        return new GameData() {
            state = GameState.none,
            playerState = PlayerState.DefaultState(),
            levelState = LevelState.Instantiate(levelTemplate),
            overlayIndex = 0,
            unlockedLevels = new List<string>{
                "Jack That Data"
            }
        };
    }
}
