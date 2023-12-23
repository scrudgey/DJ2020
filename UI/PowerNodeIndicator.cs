using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerNodeIndicator : NodeIndicator<PowerNode, PowerGraph> {
    public Color unpoweredColor;
    [Header("sprites")]
    public Sprite normalIcon;
    public Sprite powerIcon;
    public Sprite mainsIcon;
    public override void SetGraphicalState(PowerNode node) {
        switch (node.type) {
            case NodeType.none:
                iconImage.sprite = normalIcon;
                break;
            case NodeType.powerSource:
                iconImage.sprite = powerIcon;
                break;
        }

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
