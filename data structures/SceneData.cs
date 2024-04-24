using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SkyBoxType { none, city }


[CreateAssetMenu(menuName = "ScriptableObjects/SceneData")]
public class SceneData : ScriptableObject {
    public SkyBoxType skyBoxType;
    public Vector3 skyboxOffset;
    public List<float> floorHeights;

    public static SceneData loadSceneData(string sceneName) {
        Debug.Log($"load scenedata data/sceneData/{sceneName}");
        return Resources.Load<SceneData>($"data/sceneData/{sceneName}") as SceneData;
    }
    public static string SceneDataPath(bool includeDataPath = true) {
        string path = includeDataPath ? Path.Combine(Application.dataPath, "Resources", "data", "sceneData") :
                                        Path.Combine("data", "sceneData");
        return path;
    }

    public int GetFloorForPosition(Vector3 position) {
        int index = -1;
        foreach (float floorHeight in floorHeights) {
            if (floorHeight >= position.y) {
                return index;
            }
            index++;
        }
        return index;
    }
}