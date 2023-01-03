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
    public SerializableDictionary<string, LevelPlan> levelPlans;
    public int overlayIndex;

    public static GameData TestInitialData() {
        LevelTemplate levelTemplate = LevelTemplate.LoadAsInstance("test");
        return new GameData() {
            state = GameState.none,
            playerState = PlayerState.DefaultState(),
            levelState = LevelState.Instantiate(levelTemplate, LevelPlan.Default()),
            overlayIndex = 0,
            unlockedLevels = new List<string>{
                "Jack That Data"
            },
            levelPlans = new SerializableDictionary<string, LevelPlan>()
        };
    }

    public LevelPlan GetLevelPlan(LevelTemplate template) {
        if (levelPlans.ContainsKey(template.levelName)) {
            return levelPlans[template.levelName];
        } else {
            LevelPlan plan = LevelPlan.Default();
            levelPlans[template.levelName] = plan;
            return plan;
        }
    }

    public void SetLevelPlan(LevelTemplate template, LevelPlan plan) {
        levelPlans[template.levelName] = plan;
    }
}
