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
    public Graph() {
        nodes = new SerializableDictionary<string, T>();
        edges = new SerializableDictionary<string, HashSet<string>>();
    }
    public void AddEdge(Node from, Node to) {
        AddLink(from, to);
        AddLink(to, from);
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


    public static U Load<V, U>(string path) where U : Graph<V, U> where V : Node {
        XmlSerializer serializer = new XmlSerializer(typeof(U));
        if (File.Exists(path)) {
            using (FileStream sceneStream = new FileStream(path, FileMode.Open)) {
                return serializer.Deserialize(sceneStream) as U;
            }
        } else {
            Debug.LogError($"power graph file not found: {path}");
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
        string scenePath = GameManager.Level.LevelDataPath(levelName);
        string prefix = PowerGraphPrefix<T, W>(typeof(W));
        return Path.Combine(scenePath, $"graph_{prefix}_{sceneName}.xml");
    }
    public static U LoadAll<V, U>(string levelName) where U : Graph<V, U> where V : Node {
        string levelPath = GameManager.Level.LevelDataPath(levelName);
        Debug.Log($"loading power graphs at {levelPath}...");
        string prefix = PowerGraphPrefix<V, U>(typeof(U));
        string[] graphPaths = Directory.GetFiles(levelPath, $"*power*xml"); // TODO: fix this
        if (graphPaths.Length == 0) {
            Debug.LogError($"no power graphs found for level {levelName} at {levelPath}...");
            return null;
        } else {
            U graph = null;
            foreach (string path in graphPaths) {
                Debug.Log($"loading {path}...");
                graph = Load<V, U>(path);
            }
            // TODO: combine graphs
            return graph;
        }
    }

    public static string PowerGraphPrefix<U, V>(Type g) where U : Node where V : Graph<U, V> {
        if (g == typeof(PowerGraph)) {
            return "power";
        } else if (g == typeof(Graph<U, V>)) {
            return "graph";
        } else {
            return "NULL";
        }
    }

}
