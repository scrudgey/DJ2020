using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class MapMarker : MonoBehaviour {
    public MapMarkerData data;

    public static string MapPath(SceneData sceneData, int floorNumber, bool includeDataPath = true, bool withExtension = true) {
        // string scenePath = LevelState.LevelDataPath(levelName, includeDataPath: includeDataPath);
        string scenePath = sceneData.SceneDataPath(includeDataPath: includeDataPath);
        string filename = withExtension ? $"map_{floorNumber}.png" : $"map_{floorNumber}";
        return Path.Combine(scenePath, "map", filename);
    }
    public static string MapMetaDataPath(SceneData sceneData, bool withExtension = true, bool includeDataPath = true) {
        // string scenePath = LevelState.LevelDataPath(levelName, includeDataPath: includeDataPath);
        string scenePath = sceneData.SceneDataPath(includeDataPath: includeDataPath);
        if (withExtension) {

        }
        string filename = withExtension ? "map_data.xml" : "map_data";
        return Path.Combine(scenePath, "map", filename);
    }
    public static void WriteMapMetaData(SceneData sceneData, List<MapMarkerData> datas) {
        XmlSerializer serializer = new XmlSerializer(typeof(List<MapMarkerData>));
        // string path = MapMarker.MapMetaDataPath(levelName);
        string path = MapMarker.MapMetaDataPath(sceneData);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, datas);
        }
    }
    public static List<MapMarkerData> LoadMapMetaData(SceneData sceneData) {
        string metaDataPath = MapMetaDataPath(sceneData, withExtension: false, includeDataPath: false);
        TextAsset textAsset = Resources.Load<TextAsset>(metaDataPath) as TextAsset;
        XmlSerializer serializer = new XmlSerializer(typeof(List<MapMarkerData>));
        if (textAsset != null) {
            using (var reader = new System.IO.StringReader(textAsset.text)) {
                return serializer.Deserialize(reader) as List<MapMarkerData>;
            }
        } else {
            Debug.LogError($"{sceneData.name} map data not readable : {textAsset}");
            return null;
        }
    }
}

[System.Serializable]
public record MapMarkerData {
    public enum MapMarkerType { decor, insertionPoint, extractionPoint, objective, pointOfInterest, guard, camera, anchor }
    public enum MapMarkerIcon { circle, arrowUp, arrowRight, arrowDown, arrowLeft, lightningBolt, door, camera }
    public string idn;
    public MapMarkerType markerType;
    public MapMarkerIcon markerIcon;
    public int floorNumber;
    public string markerName;
    public string description;
    public Vector2 position;
    public Vector3 worldPosition;

    public string ToFlavorText() {
        return $"{idn}\ntype:{markerType}\npos:{position}";
    }
}


