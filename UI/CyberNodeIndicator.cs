using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CyberNodeIndicator : NodeIndicator<CyberNode, CyberGraph> {
    public Color compromisedColor;
    protected override void SetGraphicalState(CyberNode node) {
        // Debug.Log("set cyber graphical state");
        if (node.enabled) {
            if (!node.compromised) {
                image.color = enabledColor;
            } else {
                image.color = compromisedColor;
            }
        } else {
            image.color = disabledColor;
        }

        if (node.enabled) {
            if (!node.compromised) {
                lineRenderer.material.color = enabledColor;
            } else {
                lineRenderer.material.color = compromisedColor;
            }
        } else {
            lineRenderer.material.color = disabledColor;
        }
    }

}
