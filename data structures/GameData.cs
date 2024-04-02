using System;
using System.Collections.Generic;

using System.IO;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public record GameData {
    public string filename;
    public DateTime createdAtTime;
    public DateTime lastPlayedTime;
    public float timePlayedInSeconds;
    public GamePhase phase;
    public PlayerState playerState;
    public LevelState levelState;
    public List<string> unlockedLevels;
    public List<string> completedLevels;
    public SerializableDictionary<string, LevelPlan> levelPlans;
    public MarketData marketData;
    public List<DealData> dealData;
    public List<FenceData> fenceData;
    public static GameData TestInitialData() {
        LevelTemplate levelTemplate = Resources.Load<LevelTemplate>("data/missions/test/test") as LevelTemplate;
        // DateTime.Now;
        PlayerState playerState = PlayerState.DefaultState();

        return new GameData() {
            createdAtTime = DateTime.Now,
            timePlayedInSeconds = 0,
            filename = "test",
            phase = GamePhase.none,
            playerState = playerState,
            levelState = LevelState.Instantiate(levelTemplate, LevelPlan.Default(playerState), playerState),
            completedLevels = new List<string>(),
            unlockedLevels = new List<string>{
                "Jack That Data",
                "Tower",
                "711",
                "yamachi",
                "elevator",
                "office"
            },
            levelPlans = new SerializableDictionary<string, LevelPlan>(),
            fenceData = new List<FenceData>(),
            dealData = new List<DealData>()
        };
    }

    public LevelPlan GetLevelPlan(LevelTemplate template) {
        if (levelPlans.ContainsKey(template.levelName)) {
            return levelPlans[template.levelName];
        } else {
            LevelPlan plan = LevelPlan.Default(playerState);
            levelPlans[template.levelName] = plan;
            return plan;
        }
    }

    public void SetLevelPlan(LevelTemplate template, LevelPlan plan) {
        levelPlans[template.levelName] = plan;
    }


    public void Save() {
        CreateSaveGameFolderIfMissing();
        string path = SaveGamePath();
        Debug.Log($"saving gamedata {path}...");
        try {
            using (FileStream fs = File.Open(path, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw)) {
                JsonSerializer serializer = JsonSerializer.Create();
                serializer.Serialize(jw, this);
            }
            // Debug.Log($"wrote to {path}");
        }
        catch (Exception e) {
            Debug.LogError($"error writing to file: {path} {e}");
        }
    }
    public static GameData Load(string filename) {
        CreateSaveGameFolderIfMissing();
        string path = SaveGamePath(filename);
        try {
            using (StreamReader file = File.OpenText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                GameData template = (GameData)serializer.Deserialize(file, typeof(GameData));
                return template;
            }
        }
        catch (Exception e) {
            Debug.LogError($"error reading game data: {path} {e}");
            return GameData.TestInitialData();
        }
    }

    static public void CreateSaveGameFolderIfMissing() {
        string directoryPath = SaveGameRootDirectory();
        try {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (IOException ex) {
            Debug.LogError(ex.Message);
        }
    }

    static public string SaveGameRootDirectory() => System.IO.Path.Join(Application.persistentDataPath, "saveGames");
    static public string SaveGamePath(string filename) => System.IO.Path.Join(SaveGameRootDirectory(), filename.ToLower());
    string SaveGamePath() => System.IO.Path.Join(SaveGameRootDirectory(), filename.ToLower());
}
