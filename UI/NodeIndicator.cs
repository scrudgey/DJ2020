using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class NodeIndicator<T, U> : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler where T : Node where U : Graph<T, U> {
    public Image image;
    public Image selectionIndicatorImage;
    public RectTransform rectTransform;
    public Color enabledColor;
    public Color disabledColor;
    protected bool showSelectionIndicator;
    private float selectionIndicatorTimer;
    readonly float SELECTION_TIMEOUT = 0.05f;
    public T node;
    public Action<NodeIndicator<T, U>> onMouseOver;
    public Action<NodeIndicator<T, U>> onMouseExit;
    public static Action<NodeIndicator<T, U>> staticOnMouseOver;
    public static Action<NodeIndicator<T, U>> staticOnMouseExit;
    public IGraphOverlay<U, T, NodeIndicator<T, U>> overlay;
    public void Configure(T node, Graph<T, U> graph, IGraphOverlay<U, T, NodeIndicator<T, U>> overlay) {
        this.node = node;
        this.overlay = overlay;

        // TODO: dynamically enlarge line renderer cache

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

    }
    public void ApplyDistanceEffect(float distance) {
        // float alpha = Mathf.Clamp(1f - (Mathf.Pow(distance, 2)), 0, 1);
        float alpha = Mathf.Clamp(1f - (distance * 2f), 0, 1);
        // float alpha = Mathf.Clamp(1f - distance, 0, 1);
        HSBColor newHSBColor = HSBColor.FromColor(new Color(image.color.r, image.color.g, image.color.b, alpha));
        newHSBColor.b -= distance;
        // Color newColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
        image.color = newHSBColor.ToColor();
        transform.localScale = alpha * Vector3.one;
    }
    protected virtual void SetGraphicalState(T node) {
        if (node.getEnabled()) {
            image.color = enabledColor;
        } else {
            image.color = disabledColor;
        }
    }
    public void SetScreenPosition(Vector3 position) {
        rectTransform.position = position;
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        // move this to use callbacks
        showSelectionIndicator = true;
        staticOnMouseOver?.Invoke(this);
        onMouseOver?.Invoke(this);
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        showSelectionIndicator = false;
        selectionIndicatorImage.enabled = false;
        staticOnMouseExit?.Invoke(this);
        onMouseExit?.Invoke(this);
    }
    public virtual void OnPointerClick(PointerEventData pointerEventData) {

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
