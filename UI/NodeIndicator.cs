using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeIndicator<T, U> : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler where T : Node where U : Graph<T, U> {
    public Image image;
    public Image selectionIndicatorImage;
    public RectTransform rectTransform;
    public Color enabledColor;
    public Color disabledColor;
    // public Color deadColor;
    public NodePopupBox<T> popupBox;
    public LineRenderer lineRenderer;
    private bool showSelectionIndicator;
    private float selectionIndicatorTimer;
    readonly float SELECTION_TIMEOUT = 0.05f;
    public void Configure(T node, Graph<T, U> graph) {

        // set icon
        Sprite[] icons = Resources.LoadAll<Sprite>("sprites/UI/Powericons") as Sprite[];
        switch (node.icon) {
            case NodeIcon.normal:
                image.sprite = icons[0];
                break;
            case NodeIcon.power:
                image.sprite = icons[3];
                break;
            case NodeIcon.mains:
                image.sprite = icons[7];
                break;
        }

        SetGraphicalState(node);

        // set lines
        List<Vector3> positions = new List<Vector3>();
        foreach (T neighbor in graph.Neighbors(node)) {
            positions.Add(node.position);
            positions.Add(neighbor.position);
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        // set popup box
        popupBox.Configure(node);
    }
    protected virtual void SetGraphicalState(T node) {
        if (node.enabled) {
            image.color = enabledColor;
        } else {
            image.color = disabledColor;
        }

        if (node.enabled) {
            lineRenderer.material.color = enabledColor;
        } else {
            lineRenderer.material.color = disabledColor;
        }
    }
    public void SetScreenPosition(Vector3 position) {
        rectTransform.position = position;
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        popupBox.Show();
        showSelectionIndicator = true;
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        popupBox.Hide();
        showSelectionIndicator = false;
        selectionIndicatorImage.enabled = false;
    }

    void Update() {
        if (showSelectionIndicator) {
            selectionIndicatorTimer += Time.deltaTime;
            if (selectionIndicatorTimer > SELECTION_TIMEOUT) {
                selectionIndicatorTimer -= SELECTION_TIMEOUT;
                selectionIndicatorImage.enabled = !selectionIndicatorImage.enabled;
            }
        } else {
            selectionIndicatorImage.enabled = false;
        }
    }
}
