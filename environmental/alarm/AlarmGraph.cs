using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class AlarmGraph : Graph<AlarmNode, AlarmGraph> {
    public HashSet<HashSet<string>> activeEdges = new HashSet<HashSet<string>>();

    public void Update() {
        foreach (AlarmNode node in nodes.Values) {
            node.Update();
        }
    }
    public void Refresh() {
        activeEdges = new HashSet<HashSet<string>>();
        AlarmNode[] sources = nodes.Values.Where(node => node.alarmTriggered).ToArray();
        foreach (AlarmNode source in sources) {
            if (source.getEnabled())
                DFS(source, new HashSet<HashSet<string>>(), new HashSet<string>());
        }
    }

    public bool anyAlarmTerminalActivated() => nodes.Values
            .Where(node => node.nodeType == AlarmNode.AlarmNodeType.terminal)
            .Any(node => node.alarmTriggered);

    void DFS(AlarmNode node, HashSet<HashSet<string>> visitedEdges, HashSet<string> visitedNodes) {
        if (edges.ContainsKey(node.idn))
            foreach (string neighborID in edges[node.idn]) {
                if (visitedNodes.Contains(neighborID))
                    continue;
                visitedNodes.Add(neighborID);
                visitedEdges.Add(new HashSet<string> { node.idn, neighborID });
                AlarmNode terminalNode = nodes[neighborID];
                if (!terminalNode.getEnabled())
                    continue;
                if (terminalNode.nodeType == AlarmNode.AlarmNodeType.terminal) {
                    terminalNode.alarmTriggered = true;
                    foreach (HashSet<string> pair in visitedEdges) {
                        activeEdges.Add(pair);
                    }
                } else {
                    HashSet<HashSet<string>> newEdges = visitedEdges.ToHashSet();
                    HashSet<string> newNodes = visitedNodes.ToHashSet();
                    DFS(nodes[neighborID], newEdges, newNodes);
                }
            }
    }

}
