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
    public static LevelData Load(string levelName) {
        string path = FilePath(levelName);
        LevelData data = LoadXML(path);
        return data;
    }
    public static string FilePath(string levelName) {
        string scenePath = LevelDataPath(levelName);
        return Path.Combine(scenePath, $"{levelName}.xml");
    }
    public static string LevelDataPath(string levelName) {
        string path = Path.Combine(Application.dataPath, "Resources", "data", "levels", levelName);
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    public static LevelData LoadXML(string path) {
        XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
        if (File.Exists(path)) {
            using (FileStream sceneStream = new FileStream(path, FileMode.Open)) {
                return (LevelData)serializer.Deserialize(sceneStream);
            }
        } else {
            Debug.LogError($"level data file not found: {path}");
            return null;
        }
    }
    public void WriteXML(string levelName) {
        XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
        string path = FilePath(levelName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
        Debug.Log($"serialized {levelName} {strikeTeamMaxSize}...");
    }
}