#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


[CustomEditor(typeof(LevelDataUtil))]
[CanEditMultipleObjects]
public class LevelDataUtilEditor : Editor {
    LevelDataUtil t;
    SerializedObject GetTarget;
    SerializedProperty levelData;
    SerializedProperty mapCameraProperty;
    SerializedProperty mapTextureProperty;
    SerializedProperty floorNumberProperty;
    void OnEnable() {
        t = (LevelDataUtil)target;
        GetTarget = new SerializedObject(t);
        levelData = GetTarget.FindProperty("levelData"); // Find the List in our script and create a refrence of it
        mapCameraProperty = GetTarget.FindProperty("mapCamera"); // Find the List in our script and create a refrence of it
        mapTextureProperty = GetTarget.FindProperty("mapTexture"); // Find the List in our script and create a refrence of it
        floorNumberProperty = GetTarget.FindProperty("floorNumber"); // Find the List in our script and create a refrence of it
    }
    public override void OnInspectorGUI() {
        EditorGUILayout.PropertyField(levelData);
        LevelTemplate template = (LevelTemplate)levelData.objectReferenceValue;
        if (template != null) {
            string levelName = template.levelName;
            // Debug.Log(levelName);
            if (GUILayout.Button("Write Level Data")) {
                LevelDataUtil networkUtil = (LevelDataUtil)target;
                string sceneName = SceneManager.GetActiveScene().name;

                PowerGraph powerGraph = BuildGraph<PowerGraph, PowerNode, PoweredComponent>();
                CyberGraph cyberGraph = BuildGraph<CyberGraph, CyberNode, CyberComponent>();
                AlarmGraph alarmGraph = BuildGraph<AlarmGraph, AlarmNode, AlarmComponent>();

                powerGraph.Write(levelName, sceneName);
                cyberGraph.Write(levelName, sceneName);
                alarmGraph.Write(levelName, sceneName);

                AssetDatabase.Refresh();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(floorNumberProperty);
            EditorGUILayout.PropertyField(mapCameraProperty);
            EditorGUILayout.PropertyField(mapTextureProperty);
            if (GUILayout.Button("Write map image")) {
                SaveMapData();
            }
            if (GUILayout.Button("Write map marker data")) {
                SaveMapData();
            }
        } else {
            Debug.LogError("set level template before writing level data");
        }

        GetTarget.ApplyModifiedProperties();
    }


    public T BuildGraph<T, U, V>() where T : Graph<U, T>, new() where U : Node<U>, new() where V : GraphNodeComponent<V, U> {
        T graph = new T();

        V[] components = GameObject.FindObjectsOfType<V>();
        string sceneName = SceneManager.GetActiveScene().name;

        foreach (V component in components) {
            Guid guid = Guid.NewGuid();
            string idn = guid.ToString();
            Vector3 position = component.NodePosition();
            // Debug.Log($"{idn}: {component}");
            component.idn = idn;
            component.enabled = true;
            // new node with idn
            U node = component.NewNode();
            node.sceneName = sceneName;
            graph.nodes[idn] = node;
            EditorUtility.SetDirty(component);
        }


        foreach (V component in components) {
            if (component == null)
                continue;
            foreach (V link in component.edges) {
                if (link == null)
                    continue;
                U source = graph.nodes[component.idn];
                U neighbor = graph.nodes[link.idn];
                if (component.idn == link.idn) {
                    Debug.Log($"adding self-edge disallowed: {component.idn}");
                    continue;
                }
                graph.AddEdge(source, neighbor);
            }
        }

        return graph;
    }


    public void SaveMapData() {
        // map
        Camera mapCam = (Camera)mapCameraProperty.objectReferenceValue;
        RenderTexture renderTexture = (RenderTexture)mapTextureProperty.objectReferenceValue;
        LevelTemplate template = (LevelTemplate)levelData.objectReferenceValue;
        int floorNumber = floorNumberProperty.intValue;

        SaveMapSnapshot(mapCam, renderTexture, template, floorNumber);
        SaveMapMetaData(mapCam, template);

        AssetDatabase.Refresh();
    }
    public void SaveMarkerData() {
        // map
        Camera mapCam = (Camera)mapCameraProperty.objectReferenceValue;
        LevelTemplate template = (LevelTemplate)levelData.objectReferenceValue;
        SaveMapMetaData(mapCam, template);
        AssetDatabase.Refresh();
    }

    void SaveMapSnapshot(Camera mapCam, RenderTexture renderTexture, LevelTemplate template, int floorNumber) {
        string levelName = template.levelName;
        string sceneName = SceneManager.GetActiveScene().name;

        mapCam.targetTexture = renderTexture;
        mapCam.Render();
        mapCam.targetTexture = null;
        Texture2D tex = Toolbox.RenderToTexture2D(renderTexture);
        MapMarker.WriteMapImage(levelName, sceneName, tex, floorNumber);
    }
    void SaveMapMetaData(Camera mapCam, LevelTemplate template) {
        string levelName = template.levelName;
        string sceneName = SceneManager.GetActiveScene().name;
        List<MapMarkerData> datas = new List<MapMarkerData>();
        foreach (MapMarker marker in GameObject.FindObjectsOfType<MapMarker>()) {
            Guid guid = Guid.NewGuid();
            string idn = guid.ToString();
            marker.data.position = mapCam.WorldToViewportPoint(marker.transform.position);
            marker.data.idn = idn;
            marker.data.worldPosition = marker.transform.position;
            datas.Add(marker.data);
            EditorUtility.SetDirty(marker);
        }
        Debug.Log($"writing {datas.Count} map marker data...");
        MapMarker.WriteMapMetaData(levelName, sceneName, datas);


        Vector3 mapOrigin = mapCam.WorldToViewportPoint(Vector3.zero);
        Vector3 mapNorthPoint = mapCam.WorldToViewportPoint(new Vector3(1f, 0f, 0f));
        Vector3 mapEastPoint = mapCam.WorldToViewportPoint(new Vector3(0f, 0f, 1f));
        template.mapOrigin = mapOrigin;
        template.mapUnitNorth = mapNorthPoint - mapOrigin;
        template.mapUnitEast = mapEastPoint - mapOrigin;

        EditorUtility.SetDirty(template);
    }

}
#endif