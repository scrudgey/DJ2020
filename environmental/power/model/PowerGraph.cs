using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class PowerGraph {

    public SerializableDictionary<string, PowerNode> nodes;
    public SerializableDictionary<string, HashSet<string>> edges;
    public PowerGraph() {
        nodes = new SerializableDictionary<string, PowerNode>();
        edges = new SerializableDictionary<string, HashSet<string>>();
    }
    public void AddEdge(PowerNode from, PowerNode to) {
        AddLink(from, to);
        AddLink(to, from);
    }

    void AddLink(PowerNode from, PowerNode to) {
        if (!edges.ContainsKey(from.idn)) {
            edges[from.idn] = new HashSet<string>();
        }
        edges[from.idn].Add(to.idn);
    }

    public List<PowerNode> Neighbors(PowerNode source) {
        return edges
        .GetValueOrDefault(source.idn, new HashSet<string>())
        .Select(idn => nodes.GetValueOrDefault(idn, null))
        .Where(node => node != null)
        .ToList();
    }

    public void Refresh() {
        nodes.Values.ToList().ForEach(node => node.powered = false);

        // run the algorithm
        PowerNode[] sources = nodes.Values.Where(node => node.type == PowerNodeType.powerSource && node.enabled).ToArray();

        foreach (PowerNode source in sources) {
            // Debug.Log($"power source: {source.idn}");
            DFS(source);
        }
    }
    void DFS(PowerNode node) {
        node.powered = true;
        if (edges.ContainsKey(node.idn))
            foreach (string neighborID in edges[node.idn]) {
                if (!nodes[neighborID].powered) {
                    // Debug.Log($"propagating from {node.idn} to {neighborID}");
                    DFS(nodes[neighborID]);
                }
            }
    }

    public void Write(string levelName, string sceneName) {
        XmlSerializer serializer = new XmlSerializer(typeof(PowerGraph));
        string path = PowerGraphPath(levelName, sceneName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
    }
    public static PowerGraph LoadAll(string levelName) {
        string levelPath = GameManager.Level.LevelDataPath(levelName);
        Debug.Log($"loading power graphs at {levelPath}...");
        string[] graphPaths = Directory.GetFiles(levelPath, "powergraph*xml");
        if (graphPaths.Length == 0) {
            Debug.LogError($"no power graphs found for level {levelName} at {levelPath}...");
            return null;
        } else {
            PowerGraph graph = null;
            foreach (string path in graphPaths) {
                Debug.Log($"loading {path}...");
                graph = Load(path);
            }
            // TODO: combine graphs
            return graph;
        }
    }
    public static PowerGraph Load(string path) {
        XmlSerializer serializer = new XmlSerializer(typeof(PowerGraph));
        if (File.Exists(path)) {
            using (FileStream sceneStream = new FileStream(path, FileMode.Open)) {
                return serializer.Deserialize(sceneStream) as PowerGraph;
            }
        } else {
            Debug.LogError($"power graph file not found: {path}");
            return null;
        }
    }

    static string PowerGraphPath(string levelName, string sceneName) {
        string scenePath = GameManager.Level.LevelDataPath(levelName);
        return Path.Combine(scenePath, $"powergraph_{sceneName}.xml");
    }
}
