using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

// public enum AlarmLevel { none, alarm }

[System.Serializable]
public class LevelData {
    public PowerGraph powerGraph;
    public CyberGraph cyberGraph;
    public SensitivityLevel sensitivityLevel;
    public bool alarm;
    public float alarmCountDown;
    public bool hasHQ;
    public string alarmAudioClipPath = "sounds/alarm/klaxon";
    public int strikeTeamMaxSize;
    public float strikeTeamResponseTime;
    public static LevelData LoadLevelData(string levelName) {
        PowerGraph powerGraph = Graph<PowerNode, PowerGraph>.LoadAll(levelName);
        CyberGraph cyberGraph = Graph<CyberNode, CyberGraph>.LoadAll(levelName);
        // SensitivityLevel sensitivityLevel = SensitivityLevel.restrictedProperty;
        SensitivityLevel sensitivityLevel = SensitivityLevel.privateProperty;
        // SensitivityLevel sensitivityLevel = SensitivityLevel.semiprivateProperty;
        // SensitivityLevel sensitivityLevel = SensitivityLevel.publicProperty;
        return new LevelData {
            powerGraph = powerGraph,
            cyberGraph = cyberGraph,
            sensitivityLevel = sensitivityLevel,
            hasHQ = true,
            strikeTeamMaxSize = 3,
            strikeTeamResponseTime = 3f
            // alarmLevel = AlarmLevel.none
        };
    }
}