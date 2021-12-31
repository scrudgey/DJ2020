using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class LevelData {
    public PowerGraph powerGraph;

    public static LevelData LoadLevelData(string levelName) {

        string levelPath = GameManager.Level.LevelDataPath(levelName);

        string[] graphPaths = Directory.GetFiles(levelPath, "powergraph*");
        PowerGraph graph = null;
        foreach (string path in graphPaths) {
            Debug.Log($"loading {path}...");
            graph = PowerGraph.Load(path);
        }

        return new LevelData {
            powerGraph = graph
        };
    }
}