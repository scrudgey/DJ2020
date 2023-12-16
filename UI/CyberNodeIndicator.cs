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
    [Header("sprites")]
    public Sprite normalIcon;
    protected override void SetGraphicalState(CyberNode node) {
        iconImage.sprite = normalIcon;

        if (node.getEnabled()) {
            if (!node.compromised) {
                iconImage.color = enabledColor;
            } else {
                iconImage.color = compromisedColor;
            }
        } else {
            iconImage.color = disabledColor;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (GameManager.I.IsCyberNodeVulnerable(node)) {
            // showSelectionIndicator = false;
            audioSource.PlayOneShot(mouseOverVulnerable);

            // notify hack controller that vulnerability changed
            HackController.I.HandleVulnerableNetworkNode(node);
        } else {
            audioSource.PlayOneShot(mouseOver);
        }

        // CyberOverlay cb = (CyberOverlay)overlay;
        // cb.NodeMouseOverCallback(this);
    }
    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);

        // CyberOverlay cb = (CyberOverlay)overlay;
        // cb.NodeMouseExitCallback(this);

        // notify hack controller that vulnerability changed
        HackController.I.HandleVulnerableNetworkNode(null);
    }

    public override void OnPointerClick(PointerEventData pointerEventData) {
        base.OnPointerClick(pointerEventData);
        if (GameManager.I.IsCyberNodeVulnerable(node)) {
            HackInput input = new HackInput {
                targetNode = node,
                type = HackType.network
            };
            HackController.I.HandleHackInput(input);
        }
    }

    public List<CyberNode> GetVulnerableNodes() {
        if (GameManager.I.IsCyberNodeVulnerable(node)) {
            return new List<CyberNode> { node };
        } else return new List<CyberNode>();
    }
}
