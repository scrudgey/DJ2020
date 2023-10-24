using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkyBoxType { none, city }


[CreateAssetMenu(menuName = "ScriptableObjects/SceneData")]
public class SceneData : ScriptableObject {
    public SkyBoxType skyBoxType;
    public Vector3 skyboxOffset;

    public static SceneData loadSceneData(string sceneName) {
        Debug.Log($"load scenedata data/sceneData/{sceneName}");
        return Resources.Load<SceneData>($"data/sceneData/{sceneName}") as SceneData;
    }
}