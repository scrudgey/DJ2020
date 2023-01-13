using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public record GameData {
    public GameState state;
    public PlayerState playerState;
    public LevelState levelState;
    public List<string> unlockedLevels;
    public SerializableDictionary<string, LevelPlan> levelPlans;
    public List<string> unlockedItems;

    public static GameData TestInitialData() {
        LevelTemplate levelTemplate = LevelTemplate.LoadAsInstance("test");
        return new GameData() {
            state = GameState.none,
            playerState = PlayerState.DefaultState(),
            levelState = LevelState.Instantiate(levelTemplate, LevelPlan.Default()),
            unlockedLevels = new List<string>{
                "Jack That Data"
            },
            unlockedItems = new List<string> { "C4", "deck", "goggles", "tools" },
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
