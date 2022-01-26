using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerNodeIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Image image;
    public RectTransform rectTransform;
    public Sprite normalNode;
    public Sprite powerNode;
    public Sprite deadNode;
    public Color normalColor;
    public Color unpoweredColor;
    public Color deadColor;
    public PowerNodePopupBox popupBox;
    public LineRenderer lineRenderer;
    public void Configure(PowerNode node, PowerGraph graph) {

        // set icon
        Sprite[] icons = Resources.LoadAll<Sprite>("sprites/UI/Powericons") as Sprite[];
        switch (node.icon) {
            case PowerNodeIcon.normal:
                image.sprite = icons[0];
                break;
            case PowerNodeIcon.power:
                image.sprite = icons[3];
                break;
            case PowerNodeIcon.mains:
                image.sprite = icons[7];
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

        if (node.enabled) {
            lineRenderer.material.color = normalColor;
        } else {
            lineRenderer.material.color = unpoweredColor;
        }

        // set lines
        List<Vector3> positions = new List<Vector3>();
        foreach (PowerNode neighbor in graph.Neighbors(node)) {
            positions.Add(node.position);
            positions.Add(neighbor.position);
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        // set popup box
        popupBox.Configure(node);
    }
    public void SetScreenPosition(Vector3 position) {
        rectTransform.position = position;
    }
    public void OnPointerEnter(PointerEventData eventData) {
        popupBox.Show();
    }
    public void OnPointerExit(PointerEventData eventData) {
        popupBox.Hide();
    }
}
