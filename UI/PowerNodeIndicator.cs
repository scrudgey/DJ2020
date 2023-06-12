using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerNodeIndicator : NodeIndicator<PowerNode, PowerGraph> {
    public Color unpoweredColor;
    protected override void SetGraphicalState(PowerNode node) {
        if (node.getEnabled()) {
            if (node.powered) {
                image.color = enabledColor;
            } else {
                image.color = unpoweredColor;
            }
        } else {
            image.color = disabledColor;
        }
    }

}
