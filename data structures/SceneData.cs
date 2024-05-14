using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SkyBoxType { none, city }


[CreateAssetMenu(menuName = "ScriptableObjects/SceneData")]
public class SceneData : ScriptableObject {
    public string sceneName;
    public string sceneDescriptor;

    public SkyBoxType skyBoxType;
    public Vector3 skyboxOffset;
    public List<float> floorHeights;
    [Header("map")]
    public List<float> mapFloorHeights;
    public Vector3 mapOrigin;
    public Vector3 mapUnitNorth;
    public Vector3 mapUnitEast;

    public string SceneDataPath(bool includeDataPath = true) {
        string path = includeDataPath ? Path.Combine(Application.dataPath, "Resources", "data", "sceneData", name) :
                                        Path.Combine("data", "sceneData", name);
        // if (!Directory.Exists(path)) {
        //     Directory.CreateDirectory(path);
        // }
        return path;
    }
    public static SceneData loadSceneData(string sceneName) {
        Debug.Log($"load scenedata data/sceneData/{sceneName}/{sceneName}");
        return Resources.Load<SceneData>($"data/sceneData/{sceneName}/{sceneName}") as SceneData;
    }
    // public static string SceneDataPath(bool includeDataPath = true) {
    //     string path = includeDataPath ? Path.Combine(Application.dataPath, "Resources", "data", "sceneData") :
    //                                     Path.Combine("data", "sceneData");
    //     return path;
    // }

    public int GetCullingFloorForPosition(Vector3 position) {
        int index = -1;
        foreach (float floorHeight in floorHeights) {
            if (floorHeight >= position.y) {
                return index;
            }
            index++;
        }
        return index;
    }
    public int GetMapFloorForPosition(Vector3 position) {
        int index = -1;
        foreach (float floorHeight in mapFloorHeights) {
            if (floorHeight >= position.y) {
                return index;
            }
            index++;
        }
        return index;
    }
}