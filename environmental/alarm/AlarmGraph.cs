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

        // refresh alarm terminals
        foreach (AlarmNode node in nodes.Values) {
            AlarmComponent component = GameManager.I.GetAlarmComponent(node.idn);
            if (component is AlarmTerminal) {
                node.alarmTriggered = false;
            }
        }

        AlarmNode[] sources = nodes.Values.Where(node => node.alarmTriggered).ToArray();
        foreach (AlarmNode source in sources) {
            if (source.enabled)
                DFS(source, new HashSet<HashSet<string>>(), new HashSet<string>());
        }

        bool alarmActive = anyAlarmActive();

        if (alarmActive) {
            GameManager.I.SetLevelAlarmActive();
        } else {
            GameManager.I.DeactivateAlarm();
        }
    }

    public bool anyAlarmActive() => nodes.Values
            .Where(node => GameManager.I.GetAlarmComponent(node.idn) is AlarmTerminal)
            .Any(node => node.alarmTriggered);

    void DFS(AlarmNode node, HashSet<HashSet<string>> visitedEdges, HashSet<string> visitedNodes) {
        if (edges.ContainsKey(node.idn))
            foreach (string neighborID in edges[node.idn]) {
                if (visitedNodes.Contains(neighborID))
                    continue;
                visitedNodes.Add(neighborID);
                visitedEdges.Add(new HashSet<string> { node.idn, neighborID });
                AlarmComponent neighborComponent = GameManager.I.GetAlarmComponent(neighborID);
                AlarmNode terminalNode = GameManager.I.GetAlarmNode(neighborID);
                if (!terminalNode.enabled)
                    continue;
                if (neighborComponent is AlarmTerminal) {
                    terminalNode.alarmTriggered = true;
                    AlarmTerminal terminal = (AlarmTerminal)neighborComponent;
                    terminal.Activate();
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
