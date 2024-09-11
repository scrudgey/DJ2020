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
    public HashSet<(string, string)> edgePairs;
    public SerializableDictionary<(string, string), EdgeVisibility> edgeVisibility;
    public HashSet<(string, string)> disabledEdges;
    public Graph() {
        nodes = new SerializableDictionary<string, T>();
        edgePairs = new HashSet<(string, string)>();
        edges = new SerializableDictionary<string, HashSet<string>>();
        edgeVisibility = new SerializableDictionary<(string, string), EdgeVisibility>();
        disabledEdges = new HashSet<(string, string)>();
    }
    public T GetNode(string idn) {
        return nodes.ContainsKey(idn) ? nodes[idn] : null;
    }
    public void AddEdge(Node<T> from, Node<T> to) {
        if (from == to) return;
        AddLink(from, to);
        AddLink(to, from);
        edgePairs.Add((from.idn, to.idn));
        edgePairs.Add((to.idn, from.idn));
    }
    public void RemoveEdge(Node<T> from, Node<T> to) {
        if (from == to) return;
        RemoveLink(from, to);
        RemoveLink(to, from);
        edgePairs.Remove((from.idn, to.idn));
        edgePairs.Remove((to.idn, from.idn));
        // edgeArrays.Remove(new string[2] { from.idn, to.idn });
    }
    public void DisableEdge(string fromId, string toId) {
        disabledEdges.Add((fromId, toId));
        disabledEdges.Add((toId, fromId));
    }
    public HashSet<string> EnabledEdges(string fromId) {
        HashSet<string> output = new HashSet<string>();
        if (edges.ContainsKey(fromId)) {
            foreach (string toId in edges[fromId]) {
                if (!disabledEdges.Contains((fromId, toId))) {
                    output.Add(toId);
                }
            }
        }
        return output;
    }
    public List<T> Neighbors(Node<T> source) {
        return edges
        .GetValueOrDefault(source.idn, new HashSet<string>())
        .Where(idn => !disabledEdges.Contains((source.idn, idn)))
        .Select(idn => nodes.GetValueOrDefault(idn, null))
        .Where(node => node != null)
        .ToList();
    }
    void AddLink(Node<T> from, Node<T> to) {
        if (from == to) return;
        if (!edges.ContainsKey(from.idn)) {
            edges[from.idn] = new HashSet<string>();
        }
        edges[from.idn].Add(to.idn);
        SetEdgeVisibility(from.idn, to.idn, EdgeVisibility.known);
    }
    void RemoveLink(Node<T> from, Node<T> to) {
        if (from == to) return;
        if (!edges.ContainsKey(from.idn)) {
            // edges[from.idn] = new HashSet<string>();
            return;
        }
        edges[from.idn].Remove(to.idn);
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
        // Debug.Log($"loading all graphs at {levelPath}...");
        string prefix = PowerGraphPrefix();

        UnityEngine.Object[] graphTextAssets = Resources.LoadAll(levelPath, typeof(TextAsset));
        int graphCount = 0;
        W graph = null;
        foreach (UnityEngine.Object obj in graphTextAssets) {
            TextAsset textAsset = (TextAsset)obj;
            if (textAsset.name.ToLower().StartsWith($"graph_{prefix}")) {
                // Debug.Log($"loading {textAsset.name}...");
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
        foreach ((string, string) edgePair in rhs.edgePairs) {
            lhs.edgePairs.Add(edgePair);
        }
        return lhs;
    }


    public void ApplyNodeVisibility(Dictionary<string, NodeVisibility> visibilityData) {
        foreach (KeyValuePair<string, NodeVisibility> kvp in visibilityData) {
            if (nodes.ContainsKey(kvp.Key)) {
                nodes[kvp.Key].visibility = kvp.Value;
            }
        }
    }


    public void ApplyEdgeVisibility(LevelTemplate.GraphVisibilityDefault visibilityDefault) {

        // initialize edge visibility for all edges
        foreach (KeyValuePair<string, T> kvp in nodes) {
            if (edges.ContainsKey(kvp.Value.idn))
                foreach (string neighborId in edges[kvp.Value.idn]) {
                    SetEdgeVisibility(kvp.Value.idn, neighborId, EdgeVisibility.unknown);
                }
        }

        // apply level default
        switch (visibilityDefault) {
            case LevelTemplate.GraphVisibilityDefault.all:
                foreach ((string, string) edge in edgePairs) {
                    SetEdgeVisibility(edge.Item1, edge.Item2, EdgeVisibility.known);
                }
                break;
            case LevelTemplate.GraphVisibilityDefault.none:
                foreach (KeyValuePair<string, T> kvp in nodes) {
                    kvp.Value.visibility = NodeVisibility.unknown;
                }
                foreach ((string, string) edge in edgePairs) {
                    SetEdgeVisibility(edge.Item1, edge.Item2, EdgeVisibility.unknown);
                }
                break;
            case LevelTemplate.GraphVisibilityDefault.partial:
                foreach (KeyValuePair<string, T> kvp in nodes) {
                    kvp.Value.visibility = UnityEngine.Random.Range(0f, 1f) switch {
                        < 0.3f => NodeVisibility.unknown,
                        _ => NodeVisibility.known,
                    };
                }
                foreach ((string, string) edge in edgePairs) {
                    EdgeVisibility vis = UnityEngine.Random.Range(0f, 1f) switch {
                        < 0.3f => EdgeVisibility.unknown,
                        _ => EdgeVisibility.known,
                    };
                    SetEdgeVisibility(edge.Item1, edge.Item2, vis);
                }
                break;
        }

        // if nodes have fixed visibility, set edge visibility to known
        foreach (KeyValuePair<string, T> kvp in nodes) {
            if (kvp.Value.fixedVisibility) {
                kvp.Value.visibility = NodeVisibility.known;
                if (edges.ContainsKey(kvp.Value.idn))
                    foreach (string neighborId in edges[kvp.Value.idn]) {
                        SetEdgeVisibility(kvp.Value.idn, neighborId, EdgeVisibility.known);
                    }
            }
        }

        // ensure all nodes and edges respect minimum visibility
        foreach (KeyValuePair<string, T> kvp in nodes) {
            T node = kvp.Value;
            if (edges.ContainsKey(node.idn))
                foreach (string neighborId in edges[node.idn]) {
                    EdgeVisibility visibility = edgeVisibility[(node.idn, neighborId)];
                    visibility = (EdgeVisibility)Math.Max((int)visibility, (int)node.minimumEdgeVisibility);
                    edgeVisibility[(node.idn, neighborId)] = visibility;
                }
            NodeVisibility nodeVisibility = node.visibility;
            nodeVisibility = (NodeVisibility)Math.Max((int)nodeVisibility, (int)node.minimumNodeVisibility);
            node.visibility = nodeVisibility;
        }
    }
    public void Apply(LevelPlan levelPlan) {
        // TODO: apply plan paydata
        // TODO: set edge visibility

        // apply plan visibility
        ApplyNodeVisibility(levelPlan.nodeVisibility);
    }

    // TODO: apply randomizer
    public void SetEdgeVisibility(string id1, string id2, EdgeVisibility visibility) {
        edgeVisibility[(id1, id2)] = visibility;
        edgeVisibility[(id2, id1)] = visibility;
        if (visibility == EdgeVisibility.known) {
            if (nodes[id1].visibility < NodeVisibility.known) {
                nodes[id1].visibility = NodeVisibility.known;
            }
            if (nodes[id2].visibility < NodeVisibility.known) {
                nodes[id2].visibility = NodeVisibility.known;
            }
        }
    }

    public bool AllEdgesVisible(string idn) {
        if (edges.ContainsKey(idn)) {
            return edges[idn].All(neighbor => edgeVisibility[(idn, neighbor)] > EdgeVisibility.unknown);
        }
        return true;
    }

    public virtual bool DiscoverNode(T node,
                            NodeVisibility newNodeVisibility = NodeVisibility.unknown,
                            bool discoverEdges = false,
                            bool discoverFile = false) {
        bool doSfx = false;
        if (newNodeVisibility > node.visibility) {
            node.visibility = newNodeVisibility;
            doSfx = true;
            foreach (ObjectiveDelta objective in GameManager.I.gameData.levelState.delta.AllObjectives()) {
                if (objective.targetIdn == node.idn) {
                    objective.visibility = Objective.Visibility.known;
                }
            }
        }
        if (discoverEdges) { // TODO: only return true if there are undiscovered edges
            // doSfx = true;
            foreach (string neighborId in edges[node.idn]) {
                if (edgeVisibility[(node.idn, neighborId)] < EdgeVisibility.known) doSfx = true;
                SetEdgeVisibility(node.idn, neighborId, EdgeVisibility.known);
            }
        }
        return doSfx;
    }
}
