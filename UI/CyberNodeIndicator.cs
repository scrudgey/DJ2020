using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CyberNodeIndicator : NodeIndicator<CyberNode, CyberGraph> {
    public Color compromisedColor;
    public AudioSource audioSource;
    public AudioClip mouseOver;
    public AudioClip mouseOverVulnerable;

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
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (GameManager.I.IsCyberNodeVulnerable(node)) {
            showSelectionIndicator = false;
            audioSource.PlayOneShot(mouseOverVulnerable);
        } else {
            audioSource.PlayOneShot(mouseOver);
        }

        CyberOverlay cb = (CyberOverlay)overlay;
        cb.NodeMouseOverCallback(this);
    }
    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);

        CyberOverlay cb = (CyberOverlay)overlay;
        cb.NodeMouseExitCallback(this);
    }

    public override void OnPointerClick(PointerEventData pointerEventData) {
        base.OnPointerClick(pointerEventData);
        if (GameManager.I.IsCyberNodeVulnerable(node)) {
            HackInput input = new HackInput {
                targetNode = node
            };
            HackController.I.HandleHackInput(input);
        }
    }

}
