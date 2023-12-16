using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerNodeInfoDisplay : NodeInfoPaneDisplay<PowerGraph, PowerNode, PowerNodeIndicator> {
    public TextMeshProUGUI nodeTypeText;
    public TextMeshProUGUI poweredText;
    public override void ConfigureNode() {
        if (node.type == NodeType.powerSource) {
            nodeTypeText.text = $"power source: y";
        } else {
            nodeTypeText.text = $"power source: n";
        }
        if (node.powered) {
            poweredText.text = "powered";
        } else {
            poweredText.text = "unpowered";
        }
    }
}
