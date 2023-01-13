using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
[System.Serializable]
public class LevelState {
    public LevelTemplate template;
    public LevelPlan plan;
    public LevelDelta delta;

    public static LevelState Instantiate(LevelTemplate template, LevelPlan plan) => new LevelState {
        template = template,
        plan = plan,
        delta = LevelDelta.Empty() with {
            phase = LevelDelta.MissionPhase.action,
            powerGraph = PowerGraph.LoadAll(template.levelName),
            cyberGraph = CyberGraph.LoadAll(template.levelName),
            alarmGraph = AlarmGraph.LoadAll(template.levelName),
            strikeTeamMaxSize = 3,
            disguise = plan.startWithDisguise(),
            objectivesState = template.objectives
                .ToDictionary(t => t, t => ObjectiveStatus.inProgress)
        }
    };

    public static LevelState Instantiate(LevelTemplate template, LevelDelta delta) => new LevelState {
        template = template,
        delta = delta
    };

    public bool anyAlarmActive() {
        return delta.alarmGraph.anyAlarmActive();
    }

    public static string LevelDataPath(string levelName, bool includeDataPath = true) {
        string path = includeDataPath ? Path.Combine(Application.dataPath, "Resources", "data", "levels", levelName) :
                                        Path.Combine("data", "levels", levelName);
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        return path;
    }
}