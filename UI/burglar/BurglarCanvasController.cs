using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BurglarCanvasController : MonoBehaviour {
    BurgleTargetData data;
    BurglarToolType selectedTool;
    public RawImage rawImage;
    public GameObject UIElementPrefab;
    public Transform uiElementsContainer;
    public TextMeshProUGUI selectedToolText;
    public TextMeshProUGUI captionText;
    public void Initialize(BurgleTargetData data) {
        this.data = data;

        // initialize display
        captionText.text = "";
        foreach (Transform child in uiElementsContainer) {
            Destroy(child.gameObject);
        }
        SetTool(BurglarToolType.none);

        // configure elements
        data.target.EnableAttackSurface();
        rawImage.texture = data.target.renderTexture;
        rawImage.color = Color.white;
        RectTransform containerRectTransform = uiElementsContainer.GetComponent<RectTransform>();
        foreach (AttackSurfaceElement element in data.target.GetComponentsInChildren<AttackSurfaceElement>()) {
            Rect bounds = Toolbox.GetTotalRenderBoundingBox(element.transform, data.target.attackCam, adjustYScale: false);

            GameObject obj = GameObject.Instantiate(UIElementPrefab);
            AttackSurfaceUIElement uiElement = obj.GetComponent<AttackSurfaceUIElement>();
            obj.transform.SetParent(uiElementsContainer);

            RectTransform cursorRect = obj.GetComponent<RectTransform>();
            cursorRect.anchorMin = Vector2.zero;
            cursorRect.anchorMax = Vector2.zero;
            Image cursorImage = obj.GetComponent<Image>();

            Vector3 position = data.target.attackCam.WorldToViewportPoint(element.transform.position);
            position.x *= containerRectTransform.rect.width;
            position.y *= containerRectTransform.rect.height;
            cursorRect.anchoredPosition = position;
            cursorRect.sizeDelta = new Vector2(bounds.width, bounds.height);
            cursorImage.color = Color.red;
            cursorImage.enabled = false;

            uiElement.Initialize(this, element);
        }
    }
    public void TearDown() {
        foreach (Transform child in uiElementsContainer) {
            Destroy(child.gameObject);
        }
        if (data != null)
            data.target.DisableAttackSurface();
    }

    public void DoneButtonCallback() {
        GameManager.I.CloseBurglar();
    }

    public void ClickCallback(AttackSurfaceElement element) {
        element.HandleAttack(selectedTool, data);
    }
    public void MouseOverUIElementCallback(AttackSurfaceElement element) {
        captionText.text = element.elementName;
    }
    public void MouseExitUIElementCallback(AttackSurfaceElement element) {
        captionText.text = "";
    }
    public void ToolSelectCallback(string toolName) {
        // this is just so that we can properly wire up the buttons in unity editor.
        BurglarToolType toolType = toolName switch {
            "none" => BurglarToolType.none,
            "lockpick" => BurglarToolType.lockpick,
            "probe" => BurglarToolType.probe,
            _ => BurglarToolType.none
        };
        SetTool(toolType);
    }

    void SetTool(BurglarToolType toolType) {
        selectedTool = toolType;
        selectedToolText.text = toolType.ToString();
    }
}
