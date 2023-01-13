using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class BurglarCanvasController : MonoBehaviour {
    BurgleTargetData data;
    BurglarToolType selectedTool;
    public RawImage rawImage;
    public GameObject UIElementPrefab;
    public Transform uiElementsContainer;
    public RectTransform uiElementsRectTransform;
    public TextMeshProUGUI selectedToolText;
    public TextMeshProUGUI captionText;
    public TextMeshProUGUI feedbackText;
    public RectTransform toolPoint;
    public Image probeImage;
    public Image lockpickImage;
    public Image keyImage;
    public Image screwdriverImage;
    public GameObject keyringButton;
    Coroutine jiggleCoroutine;
    bool mouseOverElement;
    bool mouseDown;
    AttackSurfaceElement selectedElement;
    bool finishing;
    SuspicionRecord suspicionTamper() => new SuspicionRecord {
        content = "tampering with equipment",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f
    };
    public void Initialize(BurgleTargetData data) {
        this.data = data;
        finishing = false;

        // initialize display
        captionText.text = "";
        feedbackText.text = "";
        foreach (Transform child in uiElementsContainer) {
            Destroy(child.gameObject);
        }
        SetTool(BurglarToolType.none);
        keyringButton.SetActive(GameManager.I.gameData.playerState.physicalKeys.Count > 0);

        // configure elements
        data.target.EnableAttackSurface();
        rawImage.texture = data.target.renderTexture;
        rawImage.color = Color.white;
        RectTransform containerRectTransform = uiElementsContainer.GetComponent<RectTransform>();
        foreach (AttackSurfaceElement element in data.target.attackElementRoot.GetComponentsInChildren<AttackSurfaceElement>()) {
            Rect bounds = Toolbox.GetTotalRenderBoundingBox(element.transform, data.target.attackCam, adjustYScale: false);

            GameObject obj = GameObject.Instantiate(UIElementPrefab);
            AttackSurfaceUIElement uiElement = obj.GetComponent<AttackSurfaceUIElement>();
            uiElement.Bind(element.gameObject);
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

    void PositionTool() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 localPoint = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiElementsRectTransform, mousePosition, null, out localPoint);
        toolPoint.anchoredPosition = localPoint;
        if (localPoint.x > uiElementsRectTransform.rect.width / -2f && localPoint.x < uiElementsRectTransform.rect.width / 2f &&
            localPoint.y > uiElementsRectTransform.rect.height / -2f && localPoint.y < uiElementsRectTransform.rect.height / 2f) {
            if (selectedTool == BurglarToolType.none) {
                GameManager.I.cursorType = CursorType.hand;
            } else {
                GameManager.I.cursorType = CursorType.gun;
            }
        } else {
            GameManager.I.cursorType = CursorType.pointer;
        }
    }

    public void UpdateWithInput(PlayerInput input) {
        mouseDown = input.mouseDown;
        PositionTool();
        if (input.mouseDown && mouseOverElement) {
            if (selectedElement != null) {
                ClickHeld(selectedElement);
            }
            if (jiggleCoroutine == null) {
                jiggleCoroutine = StartCoroutine(JiggleTool());
            }
        } else {
            if (jiggleCoroutine != null) {
                StopCoroutine(jiggleCoroutine);
                jiggleCoroutine = null;
            }
        }
        switch (selectedTool) {
            case BurglarToolType.lockpick:
            case BurglarToolType.probe:
            case BurglarToolType.screwdriver:
                GameManager.I.AddSuspicionRecord(suspicionTamper());
                break;
        }
    }
    public void DoneButtonCallback() {
        GameManager.I.CloseBurglar();
    }

    public void ClickHeld(AttackSurfaceElement element) {
        if (finishing)
            return;
        BurglarAttackResult result = element.HandleClickHeld(selectedTool, data);
        if (result.success) {
            mouseOverElement = false;
            selectedElement = null;
        }
        HandleAttackResult(result);
    }
    public void ClickDownCallback(AttackSurfaceElement element) {
        if (finishing)
            return;
        BurglarAttackResult result = element.HandleSingleClick(selectedTool, data);
        HandleAttackResult(result);
    }
    public void ClickCallback(AttackSurfaceElement element) {
        // if (finishing)
        //     return;
        // BurglarAttackResult result = element.HandleSingleClick(selectedTool, data);
        // HandleAttackResult(result);
    }
    void HandleAttackResult(BurglarAttackResult result) {
        if (result != BurglarAttackResult.None) {
            if (result.success) {
                feedbackText.text = feedbackText.text + $"\n{result.feedbackText}";
                string[] lines = feedbackText.text.Split('\n');
                int numLines = lines.Length;
                if (numLines > 3) {
                    feedbackText.text = "";
                    feedbackText.text = $"{lines[1]}\n{lines[2]}\n{lines[3]}";
                }
            }
        }
        if (result.finish) {
            finishing = true;
            StartCoroutine(WaitAndCloseMenu(1.5f));
        }
    }
    public void MouseOverUIElementCallback(AttackSurfaceElement element) {
        mouseOverElement = true;
        selectedElement = element;
        if (selectedTool == BurglarToolType.none) {
            captionText.text = $"Use {element.elementName}";
        } else {
            captionText.text = $"Use {selectedTool} on {element.elementName}";
        }
    }
    public void MouseExitUIElementCallback(AttackSurfaceElement element) {
        captionText.text = "";
        mouseOverElement = false;
        selectedElement = null;
    }
    public void MouseEnterToolButton(string toolName) {
        selectedToolText.text = toolName;
    }
    public void MouseExitToolButton() {
        selectedToolText.text = selectedTool.ToString();
    }
    public void ToolSelectCallback(string toolName) {
        // this is just so that we can properly wire up the buttons in unity editor.
        BurglarToolType toolType = toolName switch {
            "none" => BurglarToolType.none,
            "lockpick" => BurglarToolType.lockpick,
            "probe" => BurglarToolType.probe,
            "key" => BurglarToolType.key,
            "screwdriver" => BurglarToolType.screwdriver,
            _ => BurglarToolType.none
        };
        SetTool(toolType);
    }

    void SetTool(BurglarToolType toolType) {
        selectedTool = toolType;
        selectedToolText.text = toolType.ToString();
        switch (toolType) {
            case BurglarToolType.none:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                break;
            case BurglarToolType.lockpick:
                probeImage.enabled = false;
                lockpickImage.enabled = true;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                break;
            case BurglarToolType.probe:
                probeImage.enabled = true;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                break;
            case BurglarToolType.key:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = true;
                screwdriverImage.enabled = false;
                break;
            case BurglarToolType.screwdriver:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = true;
                break;
        }
    }

    IEnumerator JiggleTool() {
        while (true) {
            while (Random.Range(0f, 1f) < 0.9f) {
                yield return new WaitForEndOfFrame();
            }
            Quaternion jiggle = Quaternion.AngleAxis(Random.Range(-10f, 10f), toolPoint.forward);
            jiggle = jiggle * toolPoint.rotation;
            Vector3 euler = jiggle.eulerAngles;
            euler.z = Mathf.Clamp(euler.z, -16f, 16f);
            jiggle = Quaternion.Euler(euler.x, euler.y, euler.z);
            toolPoint.rotation = jiggle;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator WaitAndCloseMenu(float delay) {
        yield return new WaitForSecondsRealtime(delay);
        DoneButtonCallback();
    }
}
