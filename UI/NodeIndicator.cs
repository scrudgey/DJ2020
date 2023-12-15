using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class NodeIndicator<T, U> : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, INodeCameraProvider where T : Node<T> where U : Graph<T, U> {
    static readonly Vector2 HALFSIES = Vector2.one / 2f;
    public Image iconImage;
    public RectTransform rectTransform;
    public Color enabledColor;
    public Color disabledColor;
    public T node;
    public Action<NodeIndicator<T, U>> onMouseOver;
    public Action<NodeIndicator<T, U>> onMouseExit;
    public static Action<NodeIndicator<T, U>> staticOnMouseOver;
    public static Action<NodeIndicator<T, U>> staticOnMouseExit;
    public IGraphOverlay<U, T, NodeIndicator<T, U>> overlay;
    public void Configure(T node, Graph<T, U> graph, IGraphOverlay<U, T, NodeIndicator<T, U>> overlay) {
        this.node = node;
        this.overlay = overlay;
        SetGraphicalState(node);
    }
    protected virtual void SetGraphicalState(T node) {

    }
    public void SetScreenPosition(Vector3 position) {
        rectTransform.position = position;
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        staticOnMouseOver?.Invoke(this);
        onMouseOver?.Invoke(this);
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        staticOnMouseExit?.Invoke(this);
        onMouseExit?.Invoke(this);
    }
    public virtual void OnPointerClick(PointerEventData pointerEventData) {
        overlay.overlayHandler.NodeSelectCallback(this);
    }

    public CameraInput GetCameraInput() {
        CursorData data = CursorData.none;
        data.screenPositionNormalized = HALFSIES;
        return new CameraInput {
            deltaTime = Time.deltaTime,
            wallNormal = Vector3.zero,
            lastWallInput = Vector2.zero,
            crouchHeld = false,
            playerPosition = node.position,
            state = CharacterState.overlayView,
            targetData = data,
            playerDirection = Vector3.zero,
            popoutParity = PopoutParity.left,
            aimCameraRotation = Quaternion.identity,
            targetTransform = null,
            targetPosition = node.position,
            atLeftEdge = false,
            atRightEdge = false,
            currentAttackSurface = null
        };
    }
}
