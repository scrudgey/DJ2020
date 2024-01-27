using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MiniMapNodeMarker : MonoBehaviour {
    public RectTransform rectTransform;
    public Image outline;
    public Image icon;
    public int floor;
    public NodeVisibility nodeVisibility;
    Vector3 worldPosition;
    public void Configure(MarkerConfiguration configuration, MapDisplay3DGenerator generator, int floor) {
        icon.sprite = configuration.icon;
        icon.color = configuration.color;
        outline.color = configuration.color;
        this.worldPosition = configuration.worldPosition;
        this.floor = floor;
        this.nodeVisibility = configuration.nodeVisibility;
    }

    public void SetPosition(MapDisplay3DGenerator mapDisplay3Dgenerator, RectTransform parentRect) {
        Vector3 viewPosition = mapDisplay3Dgenerator.WorldToViewportPoint(worldPosition);
        rectTransform.anchoredPosition = viewPosition * parentRect.rect.width;
    }
}


public class MarkerConfiguration {
    public Sprite icon;
    public Color color;
    public Vector3 worldPosition;
    public NodeVisibility nodeVisibility;
}