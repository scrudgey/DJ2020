#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


[CustomEditor(typeof(LevelDataUtil))]
[CanEditMultipleObjects]
public class LevelDataUtilEditor : Editor {
    LevelDataUtil t;
    SerializedObject GetTarget;
    SerializedProperty levelData;
    SerializedProperty sceneDataProperty;
    SerializedProperty mapCameraProperty;
    SerializedProperty mapTextureProperty;
    SerializedProperty floorNumberProperty;
    SerializedProperty floorWidgetProperty;
    SerializedProperty gridSpacingProperty;
    SerializedProperty boundingBoxProperty;
    SerializedProperty selectedFloorPointsProperty;
    SerializedProperty findCullingIdnProperty;
    void OnEnable() {
        t = (LevelDataUtil)target;
        GetTarget = new SerializedObject(t);
        levelData = GetTarget.FindProperty("levelData"); // Find the List in our script and create a refrence of it
        sceneDataProperty = GetTarget.FindProperty("sceneData");

        mapCameraProperty = GetTarget.FindProperty("mapCamera"); // Find the List in our script and create a refrence of it
        mapTextureProperty = GetTarget.FindProperty("mapTexture"); // Find the List in our script and create a refrence of it
        floorNumberProperty = GetTarget.FindProperty("floorNumber"); // Find the List in our script and create a refrence of it
        floorWidgetProperty = GetTarget.FindProperty("floorWidget");
        gridSpacingProperty = GetTarget.FindProperty("gridSpacing");
        boundingBoxProperty = GetTarget.FindProperty("boundingBox");
        findCullingIdnProperty = GetTarget.FindProperty("findCullingIdn");
        selectedFloorPointsProperty = GetTarget.FindProperty("selectedFloorPoints");
    }
    public override void OnInspectorGUI() {
        EditorGUILayout.PropertyField(levelData);
        EditorGUILayout.PropertyField(floorWidgetProperty);
        LevelTemplate template = (LevelTemplate)levelData.objectReferenceValue;
        SceneData sceneData = (SceneData)sceneDataProperty.objectReferenceValue;

        if (template != null) {
            GUILayout.Label("Level Template");

            string levelName = template.levelName;
            EditorGUILayout.PropertyField(floorNumberProperty);
            if (GUILayout.Button("Write map image")) {
                SaveMapData(sceneData);
            }
            if (GUILayout.Button("Set map floor level")) {
                int floorNumber = floorNumberProperty.intValue;
                SetMapFloorHeight(sceneData, floorNumber);
                // EditorGUILayout.PropertyField(floorHeightsProperty);
            }
            GUILayout.Space(10);
            EditorGUILayout.Separator();
            GUILayout.Space(10);
            if (GUILayout.Button("Write all data")) {
                WriteGraphDataButtonEffect(levelName);
                SaveMapMetaData(sceneData);
                SetObjectiveData(template);
            }
            GUILayout.Space(10);
            EditorGUILayout.Separator();
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(mapCameraProperty);
            EditorGUILayout.PropertyField(mapTextureProperty);

            if (GUILayout.Button("Write Graph Data")) {
                WriteGraphDataButtonEffect(levelName);
            }
            if (GUILayout.Button("Write map marker data")) {
                SaveMapMetaData(sceneData);
            }
            if (GUILayout.Button("Write objective data")) {
                SetObjectiveData(template);
            }

            GUILayout.Space(10);
            EditorGUILayout.Separator();
            GUILayout.Space(10);

        } else {
            // Debug.LogError("set level template before writing level data");
        }


        GUILayout.Space(20);
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(sceneDataProperty);
        GUILayout.Space(10);
        if (sceneData != null) {
            GUILayout.Label("Scene Data");
            Editor.CreateEditor(sceneData).OnInspectorGUI();

            SerializedObject serializedObject = new UnityEditor.SerializedObject(sceneData);
            SerializedProperty floorHeightsProperty = serializedObject.FindProperty("floorHeights");

            if (GUILayout.Button("Set culling floor level")) {
                int floorNumber = floorNumberProperty.intValue;
                SetFloorHeight(sceneData, floorNumber);
                // EditorGUILayout.PropertyField(floorHeightsProperty);
            }

            GUILayout.Space(20);
            EditorGUILayout.PropertyField(gridSpacingProperty);
            EditorGUILayout.PropertyField(boundingBoxProperty);
            if (GUILayout.Button("Write culling data")) {
                WriteCullingData(sceneData);
            }
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(findCullingIdnProperty);
            if (GUILayout.Button("Find culling component by idn")) {
                string idn = findCullingIdnProperty.stringValue;
                foreach (CullingComponent component in GameObject.FindObjectsOfType<CullingComponent>()) {
                    if (component.idn == idn) {
                        Selection.activeGameObject = component.gameObject;
                        Debug.Log($"{component.gameObject} {component}");
                        break;
                    }
                }
            }
        }

        GetTarget.ApplyModifiedProperties();
    }

    void WriteGraphDataButtonEffect(string levelName) {
        LevelDataUtil networkUtil = (LevelDataUtil)target;
        string sceneName = SceneManager.GetActiveScene().name;

        PowerGraph powerGraph = BuildGraph<PowerGraph, PowerNode, PoweredComponent>();
        CyberGraph cyberGraph = BuildGraph<CyberGraph, CyberNode, CyberComponent>();
        AlarmGraph alarmGraph = BuildGraph<AlarmGraph, AlarmNode, AlarmComponent>();

        powerGraph.Write(levelName, sceneName);
        cyberGraph.Write(levelName, sceneName);
        alarmGraph.Write(levelName, sceneName);

        Debug.Log("wrote all graph data.");

        AssetDatabase.Refresh();
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


    public void SaveMapData(SceneData sceneData) {
        // map
        Camera mapCam = (Camera)mapCameraProperty.objectReferenceValue;
        RenderTexture renderTexture = (RenderTexture)mapTextureProperty.objectReferenceValue;
        LevelTemplate template = (LevelTemplate)levelData.objectReferenceValue;
        int floorNumber = floorNumberProperty.intValue;

        SaveMapSnapshot(renderTexture, sceneData, floorNumber);
        // SaveMapMetaData(template);

        AssetDatabase.Refresh();
    }
    public void SaveMarkerData() {
        // map
        Camera mapCam = (Camera)mapCameraProperty.objectReferenceValue;
        LevelTemplate template = (LevelTemplate)levelData.objectReferenceValue;
        SceneData sceneData = (SceneData)sceneDataProperty.objectReferenceValue;

        SaveMapMetaData(sceneData);
        AssetDatabase.Refresh();
    }

    public void SetObjectiveData(LevelTemplate template) {
        foreach (Objective objective in template.AllObjectives()) {
            objective.spawnPointLocations = new List<Vector3>();
            objective.potentialSpawnPoints = new List<string>();
        }

        foreach (ObjectiveLootSpawnpoint lootSpawnpoint in GameObject.FindObjectsOfType<ObjectiveLootSpawnpoint>()) {
            Guid guid = Guid.NewGuid();
            string idn = guid.ToString();
            lootSpawnpoint.idn = idn;

            Objective objective = lootSpawnpoint.objective;

            objective.potentialSpawnPoints.Add(idn);
            objective.spawnPointLocations.Add(lootSpawnpoint.transform.position);

            EditorUtility.SetDirty(objective);
            EditorUtility.SetDirty(lootSpawnpoint);
        }

        List<CyberComponent> cyberDataStores = GameObject.FindObjectsOfType<CyberComponent>()
            .Where(component => component.nodeType == CyberNodeType.datanode).ToList();

        foreach (ObjectiveData objective in template.AllObjectives().Where(objective => objective is ObjectiveData)) {
            objective.spawnPointLocations = cyberDataStores.Select(component => component.transform.position).ToList();
            objective.potentialSpawnPoints = cyberDataStores.Select(component => component.idn).ToList();
            EditorUtility.SetDirty(objective);
        }

        Debug.Log($"wrote {template.AllObjectives().Count} objective data");

        EditorUtility.SetDirty(template);
    }

    void SaveMapSnapshot(RenderTexture renderTexture, SceneData sceneData, int floorNumber) {
        Camera mapCam = (Camera)mapCameraProperty.objectReferenceValue;
        mapCam.targetTexture = renderTexture;
        mapCam.Render();
        mapCam.targetTexture = null;
        Texture2D tex = Toolbox.RenderToTexture2D(renderTexture);
        WriteMapImage(sceneData, tex, floorNumber);
    }
    void WriteMapImage(SceneData sceneData, Texture2D tex, int floorNumber) {
        byte[] bytes = tex.EncodeToPNG();
        string path = MapMarker.MapPath(sceneData, floorNumber);
        Debug.Log($"writing {path}...");
        System.IO.File.WriteAllBytes(path, bytes);
    }

    void SaveMapMetaData(SceneData sceneData) {
        Camera mapCam = (Camera)mapCameraProperty.objectReferenceValue;

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
        MapMarker.WriteMapMetaData(sceneData, datas);

        Vector3 mapOrigin = mapCam.WorldToViewportPoint(Vector3.zero);
        Vector3 mapNorthPoint = mapCam.WorldToViewportPoint(new Vector3(1f, 0f, 0f));
        Vector3 mapEastPoint = mapCam.WorldToViewportPoint(new Vector3(0f, 0f, 1f));
        sceneData.mapOrigin = mapOrigin;
        sceneData.mapUnitNorth = mapNorthPoint - mapOrigin;
        sceneData.mapUnitEast = mapEastPoint - mapOrigin;

        EditorUtility.SetDirty(sceneData);
    }

    void SetFloorHeight(SceneData sceneData, int floorNumber) {
        Transform floorWidget = (Transform)floorWidgetProperty.objectReferenceValue;
        while (sceneData.floorHeights.Count < floorNumber + 1) {
            sceneData.floorHeights.Add(0);
        }
        sceneData.floorHeights[floorNumber] = floorWidget.position.y;
        EditorUtility.SetDirty(sceneData);
    }
    void SetMapFloorHeight(SceneData sceneData, int floorNumber) {
        Transform floorWidget = (Transform)floorWidgetProperty.objectReferenceValue;
        while (sceneData.mapFloorHeights.Count < floorNumber + 1) {
            sceneData.mapFloorHeights.Add(0);
        }
        sceneData.mapFloorHeights[floorNumber] = floorWidget.position.y;
        EditorUtility.SetDirty(sceneData);
    }

    void WriteCullingData(SceneData sceneData) {
        RooftopZone[] rooftopZones = GameObject.FindObjectsOfType<RooftopZone>();
        foreach (RooftopZone zone in rooftopZones) {
            zone.idn = System.Guid.NewGuid().ToString();
        }

        List<Renderer> staticRenderers = GameObject.FindObjectsOfType<Renderer>()
                    .Where(renderer => renderer.gameObject.isStatic)
                    .Where(renderer => !renderer.CompareTag("noCulling"))
                    .Concat(
                        GameObject.FindObjectsOfType<SpriteRenderer>()
                            .Where(obj => obj.CompareTag("decor"))
                            .Select(obj => obj.GetComponent<Renderer>())
                    )
                    .Concat(
                        GameObject.FindObjectsOfType<TextMeshPro>()
                            .Where(tmp => tmp.gameObject.isStatic)
                            .Select(obj => obj.GetComponent<Renderer>())
                    )
                    .ToList();

        foreach (Renderer renderer in staticRenderers) {
            if (renderer == null) continue;
            CullingComponent cullingComponent;

            if (renderer.transform.GetRoot(skipHierarchyFolders: true) != null) {
                cullingComponent = Toolbox.GetOrCreateComponent<CullingComponent>(renderer.transform.GetRoot(skipHierarchyFolders: true).gameObject);
            } else {
                if (renderer.transform.root.gameObject.IsHierarchyFolder()) {
                    cullingComponent = Toolbox.GetOrCreateComponent<CullingComponent>(renderer.gameObject);
                } else {
                    cullingComponent = Toolbox.GetOrCreateComponent<CullingComponent>(renderer.transform.root.gameObject);
                }
            }

            TagSystem tagSystem = cullingComponent.GetComponent<TagSystem>();

            cullingComponent.idn = System.Guid.NewGuid().ToString();
            cullingComponent.Initialize(sceneData);
            cullingComponent.floor = sceneData.GetCullingFloorForPosition(cullingComponent.adjustedPosition);
            cullingComponent.rooftopZoneIdn = "-1";
            if (tagSystem != null && tagSystem.data.invisibleOnPlayerFloor) {
                cullingComponent.floor += 1;
            }

            EditorUtility.SetDirty(cullingComponent);

            foreach (RooftopZone zone in rooftopZones) {
                if (zone.ContainsGeometry(cullingComponent.adjustedPosition)) {
                    cullingComponent.rooftopZoneIdn = zone.idn;
                    break;
                }
            }
        }

        BoxCollider boxCollider = (BoxCollider)boundingBoxProperty.objectReferenceValue;
        float gridSpacing = gridSpacingProperty.floatValue;


        Debug.Log("instantiating culling volume...");
        CullingVolume volume = new CullingVolume(boxCollider.bounds, gridSpacing, sceneData);

        string sceneName = SceneManager.GetActiveScene().name;
        volume.Write(sceneData, sceneName);

        AssetDatabase.Refresh();
        t = (LevelDataUtil)target;
        t.selectedGrid = volume.grids[0];
    }


}
#endif