using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class LevelData {
    public PowerGraph powerGraph;

    public static LevelData LoadLevelData(string levelName) {

        PowerGraph graph = PowerGraph.LoadAll<PowerNode, PowerGraph>(levelName);

        return new LevelData {
            powerGraph = graph
        };
    }
}