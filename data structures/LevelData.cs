using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class LevelData {
    public PowerGraph powerGraph;
    public CyberGraph cyberGraph;

    public static LevelData LoadLevelData(string levelName) {

        PowerGraph powerGraph = Graph<PowerNode, PowerGraph>.LoadAll(levelName);
        CyberGraph cyberGraph = Graph<CyberNode, CyberGraph>.LoadAll(levelName);

        return new LevelData {
            powerGraph = powerGraph,
            cyberGraph = cyberGraph
        };
    }
}