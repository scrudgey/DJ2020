using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class MapMarker : MonoBehaviour {
    public MapMarkerData data;

    public static string MapPath(string levelName, string sceneName, int floorNumber, bool includeDataPath = true, bool withExtension = true) {
        string scenePath = LevelState.LevelDataPath(levelName, includeDataPath: includeDataPath);
        string filename = withExtension ? $"map_{floorNumber}.png" : $"map_{floorNumber}";
        return Path.Combine(scenePath, "map", filename);
    }
    public static string MapMetaDataPath(string levelName, string sceneName, bool withExtension = true, bool includeDataPath = true) {
        string scenePath = LevelState.LevelDataPath(levelName, includeDataPath: includeDataPath);
        if (withExtension) {

        }
        string filename = withExtension ? "map_data.xml" : "map_data";
        return Path.Combine(scenePath, "map", filename);
    }
    public static void WriteMapMetaData(string levelName, string sceneName, List<MapMarkerData> datas) {
        XmlSerializer serializer = new XmlSerializer(typeof(List<MapMarkerData>));
        string path = MapMarker.MapMetaDataPath(levelName, sceneName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, datas);
        }
    }
    public static void WriteMapImage(string levelName, string sceneName, Texture2D tex, int floorNumber) {
        byte[] bytes = tex.EncodeToPNG();
        string path = MapMarker.MapPath(levelName, sceneName, floorNumber);
        Debug.Log($"writing {path}...");
        System.IO.File.WriteAllBytes(path, bytes);
    }

    public static List<MapMarkerData> LoadMapMetaData(string levelName, string sceneName) {
        string metaDataPath = MapMetaDataPath(levelName, sceneName, withExtension: false, includeDataPath: false);
        TextAsset textAsset = Resources.Load<TextAsset>(metaDataPath) as TextAsset;
        XmlSerializer serializer = new XmlSerializer(typeof(List<MapMarkerData>));
        if (textAsset != null) {
            using (var reader = new System.IO.StringReader(textAsset.text)) {
                return serializer.Deserialize(reader) as List<MapMarkerData>;
            }
        } else {
            Debug.LogError($"{levelName} {sceneName} map data not readable : {textAsset}");
            return null;
        }
    }
    public static List<Texture2D> LoadMapImages(string levelName, string sceneName) {
        List<Texture2D> maps = new List<Texture2D>();
        for (int i = 0; i < 10; i++) {
            Texture2D map = Resources.Load<Texture2D>(MapPath(levelName, sceneName, i, includeDataPath: false, withExtension: false)) as Texture2D;
            if (map != null) maps.Add(map);
        }
        return maps;
    }
}

[System.Serializable]
public record MapMarkerData {
    public enum MapMarkerType { decor, insertionPoint, extractionPoint, objective, pointOfInterest, guard, camera }
    public enum MapMarkerIcon { circle, arrowUp, arrowRight, arrowDown, arrowLeft, lightningBolt, door, camera }
    public string idn;
    public MapMarkerType markerType;
    public MapMarkerIcon markerIcon;
    public int floorNumber;
    public string markerName;
    public string description;
    public Vector2 position;
    public Vector3 worldPosition;
}


