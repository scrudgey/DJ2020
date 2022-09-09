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
    public void  Refresh() {
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
            DFS(source, new HashSet<HashSet<string>>());
        }
        Debug.Log($"sources: {sources.Length}");

        // foreach(AlarmNode node in nodes.Values){

        // }
        bool alarmActive = nodes.Values
            .Where(node => GameManager.I.GetAlarmComponent(node.idn) is AlarmTerminal)
            .Any(node => node.alarmTriggered);
        Debug.Log($"alarm active: {alarmActive}");

        if (alarmActive) {
            Debug.Log("set level alarm active");
            GameManager.I.SetLevelAlarmActive();
        } else {
            GameManager.I.DeactivateAlarm();
        }
    }

    void DFS(AlarmNode node, HashSet<HashSet<string>> visitedEdges) {
        // this logic should change.
        if (edges.ContainsKey(node.idn))
            foreach (string neighborID in edges[node.idn]) {
                visitedEdges.Add(new HashSet<string> { node.idn, neighborID });

                AlarmComponent neighborComponent = GameManager.I.GetAlarmComponent(neighborID);
                if (neighborComponent is AlarmTerminal) {
                    AlarmNode terminalNode = GameManager.I.GetAlarmNode(neighborID);
                    terminalNode.alarmTriggered = true;
                    // terminalNode.countdownTimer = 30f;
                    AlarmTerminal terminal = (AlarmTerminal)neighborComponent;
                    terminal.Activate();
                    foreach (HashSet<string> pair in visitedEdges) {
                        activeEdges.Add(pair);
                    }
                } else {
                    HashSet<HashSet<string>> newEdges = visitedEdges.ToHashSet();
                    DFS(nodes[neighborID], newEdges);
                }
            }
    }

}
