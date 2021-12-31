using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class PowerGraph {

    public SerializableDictionary<string, PowerNode> nodes;
    public SerializableDictionary<PowerNode, HashSet<PowerNode>> edges;
    public PowerGraph() {
        nodes = new SerializableDictionary<string, PowerNode>();
        edges = new SerializableDictionary<PowerNode, HashSet<PowerNode>>();
    }
    public void AddEdge(PowerNode from, PowerNode to) {
        AddLink(from, to);
        AddLink(to, from);
    }

    void AddLink(PowerNode from, PowerNode to) {
        if (!edges.ContainsKey(from)) {
            edges[from] = new HashSet<PowerNode>();
        }
        edges[from].Add(to);
    }

    public void Refresh() {
        nodes.Values.ToList().ForEach(node => node.power = false);
        // run the algorithm
        PowerNode[] sources = nodes.Values.Where(node => node.powerSource).ToArray();
        foreach (PowerNode source in sources) {
            Debug.Log($"power source: {source.idn}");
            DFS(source);
        }
    }
    void DFS(PowerNode node) {
        node.power = true;
        if (edges.ContainsKey(node))
            foreach (PowerNode neighbor in edges[node]) {
                if (!neighbor.power)
                    DFS(neighbor);
            }
    }

    public void Write(string levelName, string sceneName) {
        XmlSerializer serializer = new XmlSerializer(typeof(PowerGraph));
        string path = PowerGraphPath(levelName, sceneName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
    }
    public static PowerGraph Load(string path) {
        // var x = new XmlDeser
        return new PowerGraph();
    }

    static string PowerGraphPath(string levelName, string sceneName) {
        string scenePath = GameManager.Level.LevelDataPath(levelName);
        return Path.Combine(scenePath, $"powergraph_{sceneName}.xml");
    }


}
