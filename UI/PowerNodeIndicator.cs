using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerNodeIndicator : NodeIndicator<PowerNode, PowerGraph> {
    public Color unpoweredColor;
    protected override void SetGraphicalState(PowerNode node) {
        if (node.enabled) {
            if (node.powered) {
                image.color = enabledColor;
                lineRenderer.material.color = enabledColor;
            } else {
                image.color = unpoweredColor;
                lineRenderer.material.color = unpoweredColor;
            }
        } else {
            image.color = disabledColor;
            lineRenderer.material.color = disabledColor;
        }
    }

}
