using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class CyberGraph : Graph<CyberNode, CyberGraph> {
    public void Refresh() {
        foreach (CyberNode node in nodes.Values) {
            CyberComponent component = GameManager.I.GetCyberComponent(node.idn);
            component.nodeEnabled = node.enabled;
        }
    }
}
