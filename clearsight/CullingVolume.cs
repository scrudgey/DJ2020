using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class CullingVolume {
    public CullingGrid[] grids;
    public List<float> floorHeights;
    public CullingVolume() { } // required for serialization
    public CullingVolume(Bounds boundingBox, float gridSpacing, LevelTemplate template) {
        grids = new CullingGrid[template.floorHeights.Count];
        this.floorHeights = template.floorHeights;
        for (int i = 0; i < template.floorHeights.Count; i++) {
            float floorHeight = template.floorHeights[i];
            grids[i] = new CullingGrid(boundingBox, gridSpacing, floorHeight);
        }
    }
    public List<CullingGridPoint> SubgridAroundWorldPoint(Vector3 position, float radius) {
        int floorIndex = GetFloorIndexPosition(position);
        return SubgridAroundWorldPoint(floorIndex, position, radius);
    }
    public List<CullingGridPoint> SubgridAroundWorldPoint(int floorIndex, Vector3 position, float radius) {
        if (floorIndex < 0 || floorIndex >= floorHeights.Count) {
            return new List<CullingGridPoint>();
        } else {
            return grids[floorIndex].SubgridAroundWorldPoint(position, radius);
        }
    }


    public int GetFloorIndexPosition(Vector3 position) {
        int index = -1;
        foreach (float floorHeight in floorHeights) {
            if (floorHeight >= position.y) {
                return index;
            }
            index++;
        }
        return index;
    }
    public int GetTransitionFloor(Vector3 position, float bufferSize) {
        int index = 0;
        foreach (float floorHeight in floorHeights) {
            if (position.y >= floorHeight - bufferSize && position.y <= floorHeight) {
                return index;
            }
            index++;
        }
        return -99;
    }
    /**
    |
    |
    |
    | floor 2
    |
    |-------- height 2
    | buffer <-----------------we are here. position.y > height 2 - buffer && position.y < height 2; this means we're between index-1 and index
    |-
    |
    | floor 1
    |
    |-------- height 1
    | buffer
    |-
    |
    | floor 0
    |
    |-------- height 0
    | buffer
    |- 
    | 
    | floor -1
     */

    public void Write(string levelName, string sceneName) {
        XmlSerializer serializer = new XmlSerializer(typeof(CullingVolume));
        string path = CullingFilePath(levelName, sceneName);
        Debug.Log($"writing culling data to {path}...");
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
    }

    public static string CullingFilePath(string levelName, string sceneName) {
        string scenePath = LevelState.LevelDataPath(levelName);
        return Path.Combine(scenePath, $"cullingvolume_{sceneName}.xml");
    }

    public static CullingVolume Load(string levelName, string sceneName) {
        string path = $"data/missions/{levelName}/cullingvolume_{sceneName}";
        TextAsset textAsset = Resources.Load<TextAsset>(path) as TextAsset;
        XmlSerializer serializer = new XmlSerializer(typeof(CullingVolume));
        Debug.Log($"loading culling volume at {path}...");
        using (var reader = new System.IO.StringReader(textAsset.text)) {
            CullingVolume cullingGrid = serializer.Deserialize(reader) as CullingVolume;
            // Debug.Log($"loaded culling grid with {cullingGrid.xRows}x{cullingGrid.zRows}...");
            return cullingGrid;
        }
    }
}