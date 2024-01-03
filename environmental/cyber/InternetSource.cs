using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetSource : CyberComponent {
    public override CyberNode NewNode() {
        CyberNode node = base.NewNode();
        node.compromised = true;
        node.type = CyberNodeType.WAN;
        node.lockLevel = 0;
        node.visibility = NodeVisibility.mapped;
        node.fixedVisibility = true;
        node.alwaysOnScreen = true;
        node.dataSink = true;
        return node;
    }
}
