using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class LevelState {
    public LevelTemplate template;
    public LevelDelta delta;

    public static LevelState Instantiate(LevelTemplate template) => new LevelState {
        template = template,
        delta = LevelDelta.Empty() with {
            powerGraph = PowerGraph.LoadAll(template.levelName),
            cyberGraph = CyberGraph.LoadAll(template.levelName),
            alarmGraph = AlarmGraph.LoadAll(template.levelName),
            strikeTeamMaxSize = 3
        }
    };

    public static LevelState Instantiate(LevelTemplate template, LevelDelta delta) => new LevelState {
        template = template,
        delta = delta
    };

    public bool anyAlarmActive() {
        return delta.alarmGraph.anyAlarmActive();
    }

    public static string LevelDataPath(string levelName) {
        string path = Path.Combine(Application.dataPath, "Resources", "data", "levels", levelName);
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        return path;
    }
}