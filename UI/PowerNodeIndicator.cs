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
    protected override void SetGraphicalState(PowerNode node) {
        switch (node.type) {
            case NodeType.none:
                image.sprite = normalIcon;
                break;
            case NodeType.powerSource:
                image.sprite = powerIcon;
                break;
        }

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
