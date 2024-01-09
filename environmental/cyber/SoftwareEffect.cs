using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class SoftwareEffect {
    public enum Type { scan, download, unlock, compromise, none }
    public Type type;
    public int level;
    public string name;
    public void ApplyToNode(CyberNode node, CyberGraph graph) {
        switch (type) {
            case Type.scan:
                node.visibility = NodeVisibility.known;
                foreach (string neighborId in graph.edges[node.idn]) {
                    graph.SetEdgeVisibility(node.idn, neighborId, EdgeVisibility.known);

                }
                break;
            case Type.download:
                if (node.type == CyberNodeType.datanode) {
                    GameManager.I.AddPayDatas(node.payData);
                    node.dataStolen = true;
                }
                break;
            case Type.unlock:
                node.lockLevel = 0;
                if (node.visibility == NodeVisibility.unknown || node.visibility == NodeVisibility.mystery) {
                    node.visibility = NodeVisibility.known;
                }
                break;
            case Type.compromise:
                node.compromised = true;
                break;
        }
    }
}