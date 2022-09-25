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
        }
    };

    public static LevelState Instantiate(LevelTemplate template, LevelDelta delta) => new LevelState {
        template = template,
        delta = delta
    };

    public bool anyAlarmActive() {
        return delta.alarmGraph.anyAlarmActive();
    }

    public static LevelState Load(string levelName) {
        string path = FilePath(levelName);
        LevelState data = LoadXML(path);
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
    public static LevelState LoadXML(string path) {
        XmlSerializer serializer = new XmlSerializer(typeof(LevelState));
        if (File.Exists(path)) {
            using (FileStream sceneStream = new FileStream(path, FileMode.Open)) {
                LevelState levelData = (LevelState)serializer.Deserialize(sceneStream);
                return levelData;
            }
        } else {
            Debug.LogError($"level data file not found: {path}");
            return null;
        }
    }
    public void WriteXML(string levelName) {
        XmlSerializer serializer = new XmlSerializer(typeof(LevelState));
        string path = FilePath(levelName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
    }
}