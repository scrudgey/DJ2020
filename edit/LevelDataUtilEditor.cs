#if UNITY_EDITOR

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
    // public string levelName = "test";
    // public LevelTemplate levelData;
    LevelDataUtil t;
    SerializedObject GetTarget;
    SerializedProperty levelData;
    void OnEnable() {
        t = (LevelDataUtil)target;
        GetTarget = new SerializedObject(t);
        levelData = GetTarget.FindProperty("levelData"); // Find the List in our script and create a refrence of it
    }
    public override void OnInspectorGUI() {

        EditorGUILayout.PropertyField(levelData);
        // levelData = (LevelTemplate)base.serializedObject.FindProperty("levelData").objectReferenceValue;

        // levelName = EditorGUILayout.TextField("level name", levelName);
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

                foreach (Node node in powerGraph.nodes.Values) {
                    Debug.Log($"writing power graph: {node.idn} {levelName}");
                }

                powerGraph.Write(levelName, sceneName);
                cyberGraph.Write(levelName, sceneName);
                alarmGraph.Write(levelName, sceneName);

                AssetDatabase.Refresh();
            }
        } else {
            Debug.LogError("set level template before writing level data");
        }

        GetTarget.ApplyModifiedProperties();
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
#endif