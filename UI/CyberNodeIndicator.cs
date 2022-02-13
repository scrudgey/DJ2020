using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CyberNodeIndicator : NodeIndicator<CyberNode, CyberGraph> {
    public Color compromisedColor;
    public VulnerabilityIndicator vulnerabilityIndicator;
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
            if (vulnerabilityIndicator.IsVisible()) {
                lineRenderer.material.color = compromisedColor;
            } else {
                lineRenderer.material.color = enabledColor;
            }
        } else {
            lineRenderer.material.color = disabledColor;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (GameManager.I.IsCyberNodeVulnerable(node)) {
            vulnerabilityIndicator.StartIndicator();
            showSelectionIndicator = false;
        }
        CyberNodeIndicator.onMouseOver?.Invoke(this);
    }
    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        vulnerabilityIndicator.StopIndicator();
        CyberNodeIndicator.onMouseExit?.Invoke(this);
    }

}
