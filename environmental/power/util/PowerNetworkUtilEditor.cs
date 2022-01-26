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
    public override void OnInspectorGUI() {
        // PowerNetworkUtil networkUtil = (PowerNetworkUtil)target;

        if (GUILayout.Button("Build Power Network")) {
            PowerGraph graph = BuildPowerNetwork();

            string levelName = "test";
            string sceneName = SceneManager.GetActiveScene().name;

            graph.Write(levelName, sceneName);
            AssetDatabase.Refresh();
        }

    }

    public PowerGraph BuildPowerNetwork() {
        PowerGraph graph = new PowerGraph();

        PoweredComponent[] components = GameObject.FindObjectsOfType<PoweredComponent>();

        foreach (var group in components.GroupBy(component => component.gameObject)) {
            Guid guid = Guid.NewGuid();
            string idn = guid.ToString();

            Vector3 position = group.First().NodePosition();

            // new node with idn
            PowerNode node = new PowerNode {
                idn = idn,
                position = position,
                enabled = true,
                icon = PowerNodeIcon.normal
            };
            graph.nodes[idn] = node;

            foreach (PoweredComponent component in group) {
                if (component.nodeTitle != "") {
                    node.nodeTitle = component.nodeTitle;
                }
                if (component.icon != PowerNodeIcon.normal)
                    node.icon = component.icon;
                Debug.Log($"{idn}: {component}");
                // set the component's id
                component.idn = idn;
                EditorUtility.SetDirty(component);
                switch (component) {
                    case PowerSource source:
                        node.type = PowerNodeType.powerSource;
                        break;
                    default:
                        node.type = PowerNodeType.normal;
                        break;
                }
            }
        }
        foreach (PoweredComponent component in components) {
            foreach (PoweredComponent link in component.edges) {
                PowerNode source = graph.nodes[component.idn];
                PowerNode neighbor = graph.nodes[link.idn];
                graph.AddEdge(source, neighbor);
            }
        }

        return graph;
    }

}