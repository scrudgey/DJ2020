using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


[CustomEditor(typeof(LevelDataUtil))]
[CanEditMultipleObjects]
public class LevelDataUtilEditor : Editor {
    // TODO: allow an audioclip in editor
    public string levelName = "test";
    public SerializedProperty klaxonSound;
    private SerializedProperty levelData;
    private void OnEnable() {
        levelData = serializedObject.FindProperty("levelData");
        klaxonSound = serializedObject.FindProperty("klaxonSound");
    }
    public override void OnInspectorGUI() {
        levelName = EditorGUILayout.TextField("level name", levelName);
        EditorGUILayout.PropertyField(klaxonSound);
        EditorGUILayout.PropertyField(levelData);

        if (GUILayout.Button("Write Level Data")) {
            LevelDataUtil networkUtil = (LevelDataUtil)target;
            string sceneName = SceneManager.GetActiveScene().name;

            PowerGraph powerGraph = BuildGraph<PowerGraph, PowerNode, PoweredComponent>();
            CyberGraph cyberGraph = BuildGraph<CyberGraph, CyberNode, CyberComponent>();
            AlarmGraph alarmGraph = BuildGraph<AlarmGraph, AlarmNode, AlarmComponent>();

            powerGraph.Write(levelName, sceneName);
            cyberGraph.Write(levelName, sceneName);
            alarmGraph.Write(levelName, sceneName);

            networkUtil.levelData.powerGraph = powerGraph;
            networkUtil.levelData.cyberGraph = cyberGraph;
            networkUtil.levelData.alarmGraph = alarmGraph;

            String klaxonRootPath = AssetDatabase.GetAssetPath(networkUtil.klaxonSound);
            String klaxonRelativePath = klaxonRootPath.Replace("Assets/Resources/", "");

            int fileExtPos = klaxonRelativePath.LastIndexOf(".");
            if (fileExtPos >= 0)
                klaxonRelativePath = klaxonRelativePath.Substring(0, fileExtPos);

            networkUtil.levelData.alarmAudioClipPath = klaxonRelativePath;

            networkUtil.levelData.WriteXML(levelName);

            AssetDatabase.Refresh();
        }
        serializedObject.ApplyModifiedProperties();
    }


    public T BuildGraph<T, U, V>() where T : Graph<U, T>, new() where U : Node, new() where V : GraphNodeComponent<V, U> {
        T graph = new T();

        V[] components = GameObject.FindObjectsOfType<V>();
        string sceneName = SceneManager.GetActiveScene().name;

        foreach (var group in components.GroupBy(component => component.gameObject)) {
            Guid guid = Guid.NewGuid();
            string idn = guid.ToString();

            Vector3 position = group.First().NodePosition();

            // new node with idn
            U node = new U {
                sceneName = sceneName,
                idn = idn,
                position = position,
                enabled = true,
                icon = NodeIcon.normal
            };
            graph.nodes[idn] = node;

            foreach (V component in group) {
                if (component.nodeTitle != "") {
                    node.nodeTitle = component.nodeTitle;
                }
                if (component.icon != NodeIcon.normal)
                    node.icon = component.icon;
                Debug.Log($"{idn}: {component}");
                // set the component's id
                component.idn = idn;
                EditorUtility.SetDirty(component);

                node.type = component switch {
                    PowerSource => NodeType.powerSource,
                    InternetSource => NodeType.WAN,
                    _ => NodeType.normal
                };

                // allow the subclass to add class-specific configuration
                component.ConfigureNode(node);
            }
        }
        foreach (V component in components) {
            if (component == null)
                continue;
            foreach (V link in component.edges) {
                if (link == null)
                    continue;
                U source = graph.nodes[component.idn];
                U neighbor = graph.nodes[link.idn];
                graph.AddEdge(source, neighbor);
            }
        }

        return graph;
    }
}
