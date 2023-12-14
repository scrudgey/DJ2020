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
    }
}
