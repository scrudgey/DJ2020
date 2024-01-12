using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerNodeIndicator : NodeIndicator<PowerNode, PowerGraph> {
    public Color unpoweredColor;
    public GraphIconReference icons;
    public override void SetGraphicalState(PowerNode node) {
        iconImage.sprite = icons.PowerNodeSprite(node);

        if (node.getEnabled()) {
            if (node.powered) {
                iconImage.color = enabledColor;
            } else {
                iconImage.color = unpoweredColor;
            }
        } else {
            iconImage.color = disabledColor;
        }
    }
}
