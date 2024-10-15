using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : PoweredComponent {
    public override PowerNode NewNode() {
        PowerNode node = base.NewNode();
        node.powered = true;
        node.type = PowerNode.NodeType.powerSource;
        return node;
    }
    public override void OnDestroy() {
        if (GameManager.I.isLoadingLevel) return;

        base.OnDestroy();
        GameManager.I?.SetPowerNodeState(this, false);
    }
}
