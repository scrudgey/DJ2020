using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(PowerNetworkUtil))]
public class PowerNetworkUtilEditor : Editor {
    public string levelName = "test";
    public override void OnInspectorGUI() {
        // PowerNetworkUtil networkUtil = (PowerNetworkUtil)target;

        if (GUILayout.Button("Build Power Network")) {
            string sceneName = SceneManager.GetActiveScene().name;

            PowerGraph powerGraph = BuildGraph<PowerGraph, PowerNode, PoweredComponent>();
            CyberGraph cyberGraph = BuildGraph<CyberGraph, CyberNode, CyberComponent>();

            powerGraph.Write(levelName, sceneName);
            cyberGraph.Write(levelName, sceneName);
            AssetDatabase.Refresh();
        }

    }

    public T BuildGraph<T, U, V>() where T : Graph<U, T>, new() where U : Node, new() where V : GraphNodeComponent<V> {
        T graph = new T();

        V[] components = GameObject.FindObjectsOfType<V>();

        foreach (var group in components.GroupBy(component => component.gameObject)) {
            Guid guid = Guid.NewGuid();
            string idn = guid.ToString();

            Vector3 position = group.First().NodePosition();

            // new node with idn
            U node = new U {
                idn = idn,
                position = position,
                enabled = true,
                icon = PowerNodeIcon.normal
            };
            graph.nodes[idn] = node;

            foreach (V component in group) {
                if (component.nodeTitle != "") {
                    node.nodeTitle = component.nodeTitle;
                }
                if (component.icon != PowerNodeIcon.normal)
                    node.icon = component.icon;
                Debug.Log($"{idn}: {component}");
                // set the component's id
                component.idn = idn;
                EditorUtility.SetDirty(component);

                node.type = component switch {
                    PowerSource => PowerNodeType.powerSource,
                    _ => PowerNodeType.normal
                };
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