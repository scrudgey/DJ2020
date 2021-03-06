using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class PowerGraph : Graph<PowerNode, PowerGraph> {
    public void Refresh() {
        nodes.Values.ToList().ForEach(node => node.powered = false);

        // run the algorithm
        PowerNode[] sources = nodes.Values.Where(node => node.type == NodeType.powerSource && node.enabled).ToArray();

        foreach (PowerNode source in sources) {
            // Debug.Log($"power source: {source.idn}");
            DFS(source);
        }
    }
    void DFS(PowerNode node) {
        node.powered = true;
        if (node.enabled && edges.ContainsKey(node.idn))
            foreach (string neighborID in edges[node.idn]) {
                if (!nodes[neighborID].powered) {
                    // Debug.Log($"propagating from {node.idn} to {neighborID}");
                    DFS(nodes[neighborID]);
                }
            }
    }
}
