using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerNodeIndicator : MonoBehaviour {
    public Image image;
    public RectTransform rectTransform;
    public Sprite normalNode;
    public Sprite powerNode;
    public Sprite deadNode;
    public Color normalColor;
    public Color unpoweredColor;
    public Color deadColor;
    public void Configure(PowerNode node) {
        switch (node.type) {
            default:
                image.sprite = normalNode;
                break;
            case PowerNodeType.powerSource:
                image.sprite = powerNode;
                break;
        }
        if (node.enabled) {
            if (node.powered) {
                image.color = normalColor;
            } else {
                image.color = unpoweredColor;
            }
        } else {
            image.color = deadColor;
        }
    }
    public void SetScreenPosition(Vector3 position) {
        rectTransform.position = position;
    }
}
