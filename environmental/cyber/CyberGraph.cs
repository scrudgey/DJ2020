using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class CyberGraph : Graph<CyberNode, CyberGraph> {
    public bool IsCyberNodeVulnerable(CyberNode node) {
        if (node.compromised)
            return false;
        if (nodes.ContainsKey(node.idn)) {
            foreach (CyberNode neighbor in Neighbors(node)) {
                if (neighbor.compromised) return true;
            }
            return false;
        } else return false;
    }

    public CyberNodeStatus GetStatus(CyberNode node) {
        if (node.compromised) {
            return CyberNodeStatus.compromised;
        } else {
            if (IsCyberNodeVulnerable(node)) {
                return CyberNodeStatus.vulnerable;
            } else {
                return CyberNodeStatus.invulnerable;
            }
        }
    }

    public void Refresh() {
        foreach (CyberNode node in nodes.Values) {
            node.status = GetStatus(node);
        }
        foreach (KeyValuePair<string, CyberNode> kvp in nodes) {
            if (kvp.Value.visibility == NodeVisibility.unknown) {
                if (edges.ContainsKey(kvp.Key) && edges[kvp.Key].Any(idn => nodes[idn].visibility == NodeVisibility.mapped)) {
                    kvp.Value.visibility = NodeVisibility.mystery;
                }
            }
        }
    }


    public void ApplyPayDataState(Dictionary<string, PayData> paydataData) {
        foreach (KeyValuePair<string, PayData> kvp in paydataData) {
            if (nodes.ContainsKey(kvp.Key)) {
                nodes[kvp.Key].payData = kvp.Value;
            }
        }
    }

    public void InfillRandomData() {
        foreach (KeyValuePair<string, CyberNode> kvp in nodes) {
            kvp.Value.payData = PayData.RandomPaydata();
        }
    }

    public void ApplyObjectiveData(List<ObjectiveData> objectives) {
        List<CyberNode> dataNodes = nodes.Values.Where(node => node.type == CyberNodeType.datanode).ToList();
        foreach (ObjectiveData objective in objectives) {
            if (dataNodes.Count == 0) {
                Debug.LogError("Not enough data nodes to support level objectives! Mission is not possible.");
                break;
            }
            CyberNode target = Toolbox.RandomFromList(dataNodes);
            target.payData = objective.targetPaydata;
            dataNodes.Remove(target);
        }
    }
}
