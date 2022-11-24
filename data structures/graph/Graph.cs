using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class Graph<T, W> where T : Node where W : Graph<T, W> {
    public SerializableDictionary<string, T> nodes;
    public SerializableDictionary<string, HashSet<string>> edges;
    public HashSet<HashSet<string>> edgePairs;
    public Graph() {
        nodes = new SerializableDictionary<string, T>();
        edgePairs = new HashSet<HashSet<string>>(HashSet<string>.CreateSetComparer());
        edges = new SerializableDictionary<string, HashSet<string>>();
    }

    public void AddEdge(Node from, Node to) {
        AddLink(from, to);
        AddLink(to, from);
        edgePairs.Add(new HashSet<string> { from.idn, to.idn });
    }

    void AddLink(Node from, Node to) {
        if (!edges.ContainsKey(from.idn)) {
            edges[from.idn] = new HashSet<string>();
        }
        edges[from.idn].Add(to.idn);
    }

    public List<T> Neighbors(Node source) {
        return edges
        .GetValueOrDefault(source.idn, new HashSet<string>())
        .Select(idn => nodes.GetValueOrDefault(idn, null))
        .Where(node => node != null)
        .ToList();
    }


    public static W Load(TextAsset textAsset) {
        XmlSerializer serializer = new XmlSerializer(typeof(W));
        if (textAsset != null) {
            using (var reader = new System.IO.StringReader(textAsset.text)) {
                return serializer.Deserialize(reader) as W;
            }
        } else {
            Debug.LogError($"power graph file not readable: {textAsset}");
            return null;
        }
    }

    public void Write(string levelName, string sceneName) {
        XmlSerializer serializer = new XmlSerializer(typeof(W));
        string path = FilePath(levelName, sceneName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
    }

    private string FilePath(string levelName, string sceneName) {
        string scenePath = LevelState.LevelDataPath(levelName);
        string prefix = PowerGraphPrefix();
        return Path.Combine(scenePath, $"graph_{prefix}_{sceneName}.xml");
    }

    public static W LoadAll(string levelName) {
        string levelPath = Path.Combine("data", "levels", levelName);
        Debug.Log($"loading all graphs at {levelPath}...");
        string prefix = PowerGraphPrefix();

        UnityEngine.Object[] graphTextAssets = Resources.LoadAll(levelPath, typeof(TextAsset));
        int graphCount = 0;
        W graph = null;
        foreach (UnityEngine.Object obj in graphTextAssets) {
            TextAsset textAsset = (TextAsset)obj;
            if (textAsset.name.ToLower().StartsWith($"graph_{prefix}")) {
                Debug.Log($"loading {textAsset.name}...");
                if (graph is null) {
                    graph = Load(textAsset);
                } else {
                    graph = graph + Load(textAsset) as W;
                }
                graphCount += 1;
            }

        }
        if (graphCount == 0) {
            Debug.LogError($"no graphs found for level {levelName} at {levelPath} with prefix {prefix}...");
        }
        return graph;
    }

    public static string PowerGraphPrefix() {
        if (typeof(W) == typeof(PowerGraph)) {
            return "power";
        } else if (typeof(W) == typeof(CyberGraph)) {
            return "cyber";
        } else if (typeof(W) == typeof(AlarmGraph)) {
            return "alarm";
        } else {
            return "generic";
        }
    }


    public static Graph<T, W> operator +(Graph<T, W> lhs, Graph<T, W> rhs) {
        foreach (KeyValuePair<string, T> node in rhs.nodes) {
            lhs.nodes[node.Key] = node.Value;
        }
        foreach (KeyValuePair<string, HashSet<string>> edge in rhs.edges) {
            lhs.edges[edge.Key] = edge.Value;
        }
        foreach (HashSet<string> edgePair in rhs.edgePairs) {
            lhs.edgePairs.Add(edgePair);
        }
        return lhs;
    }

}
