using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class Graph<T, W> where T : Node<T> where W : Graph<T, W> {
    public string levelName;
    public SerializableDictionary<string, T> nodes;
    public SerializableDictionary<string, HashSet<string>> edges;
    public HashSet<HashSet<string>> edgePairs;
    public HashSet<string[]> edgeArrays;
    public Graph() {
        nodes = new SerializableDictionary<string, T>();
        edgePairs = new HashSet<HashSet<string>>(HashSet<string>.CreateSetComparer());
        edgeArrays = new HashSet<string[]>();
        edges = new SerializableDictionary<string, HashSet<string>>();
    }
    public T GetNode(string idn) {
        return nodes.ContainsKey(idn) ? nodes[idn] : null;
    }
    public void AddEdge(Node<T> from, Node<T> to) {
        AddLink(from, to);
        AddLink(to, from);
        edgePairs.Add(new HashSet<string> { from.idn, to.idn });
        edgeArrays.Add(new string[2] { from.idn, to.idn });
    }

    void AddLink(Node<T> from, Node<T> to) {
        if (!edges.ContainsKey(from.idn)) {
            edges[from.idn] = new HashSet<string>();
        }
        edges[from.idn].Add(to.idn);
    }

    public List<T> Neighbors(Node<T> source) {
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
        string levelPath = Path.Combine("data", "missions", levelName);
        Debug.Log($"loading all graphs at {levelPath}...");
        string prefix = PowerGraphPrefix();

        UnityEngine.Object[] graphTextAssets = Resources.LoadAll(levelPath, typeof(TextAsset));
        int graphCount = 0;
        W graph = null;
        foreach (UnityEngine.Object obj in graphTextAssets) {
            TextAsset textAsset = (TextAsset)obj;
            if (textAsset.name.ToLower().StartsWith($"graph_{prefix}")) {
                Debug.Log($"loading {textAsset.name}...");
                graph = graph == null ? Load(textAsset) : graph + Load(textAsset) as W;
                graphCount += 1;
            }
        }
        if (graphCount == 0) {
            Debug.LogError($"no graphs found for level {levelName} at {levelPath} with prefix {prefix}...");
        }
        if (graph != null)
            graph.levelName = levelName;
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
            lhs.edgeArrays.Add(edgePair.ToArray());
        }
        return lhs;
    }


    public void ApplyVisibilityState(Dictionary<string, NodeVisibility> visibilityData) {
        foreach (KeyValuePair<string, NodeVisibility> kvp in visibilityData) {
            if (nodes.ContainsKey(kvp.Key)) {
                nodes[kvp.Key].visibility = kvp.Value;
            }
        }
    }


    public void Apply(LevelTemplate.GraphVisibilityDefault visibilityDefault) {
        switch (visibilityDefault) {
            case LevelTemplate.GraphVisibilityDefault.all:
                foreach (KeyValuePair<string, T> kvp in nodes) {
                    if (kvp.Value.fixedVisibility) continue;
                    kvp.Value.visibility = NodeVisibility.mapped;
                }
                break;
            case LevelTemplate.GraphVisibilityDefault.none:
                foreach (KeyValuePair<string, T> kvp in nodes) {
                    if (kvp.Value.fixedVisibility) continue;
                    kvp.Value.visibility = NodeVisibility.unknown;
                }
                break;
            case LevelTemplate.GraphVisibilityDefault.partial:
                foreach (KeyValuePair<string, T> kvp in nodes) {
                    if (kvp.Value.fixedVisibility) continue;
                    kvp.Value.visibility = UnityEngine.Random.Range(0f, 1f) switch {
                        < 0.3f => NodeVisibility.unknown,
                        < 0.6f => NodeVisibility.known,
                        _ => NodeVisibility.mapped
                    };
                }
                break;
        }
    }
    public void Apply(LevelPlan levelPlan) {
        // TODO: apply plan paydata
        // apply plan visibility
        ApplyVisibilityState(levelPlan.nodeVisibility);
    }

    // TODO: apply randomizer
}
