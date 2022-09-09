using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;


[System.Serializable]
public class LevelData {
    public PowerGraph powerGraph;
    public CyberGraph cyberGraph;
    public AlarmGraph alarmGraph;
    public SensitivityLevel sensitivityLevel;
    public string alarmAudioClipPath = "sounds/alarm/klaxon";
    public int strikeTeamMaxSize;
    public float strikeTeamResponseTime;
    public bool anyAlarmActive() {
        return alarmGraph.anyAlarmActive();
    }
    public static LevelData LoadLevelData(string levelName) {
        PowerGraph powerGraph = Graph<PowerNode, PowerGraph>.LoadAll(levelName);
        CyberGraph cyberGraph = Graph<CyberNode, CyberGraph>.LoadAll(levelName);
        AlarmGraph alarmGraph = Graph<AlarmNode, AlarmGraph>.LoadAll(levelName);

        // SensitivityLevel sensitivityLevel = SensitivityLevel.restrictedProperty;
        SensitivityLevel sensitivityLevel = SensitivityLevel.privateProperty;
        // SensitivityLevel sensitivityLevel = SensitivityLevel.semiprivateProperty;
        // SensitivityLevel sensitivityLevel = SensitivityLevel.publicProperty;
        return new LevelData {
            powerGraph = powerGraph,
            cyberGraph = cyberGraph,
            alarmGraph = alarmGraph,
            sensitivityLevel = sensitivityLevel,
            strikeTeamMaxSize = 3,
            strikeTeamResponseTime = 3f
        };
    }
}