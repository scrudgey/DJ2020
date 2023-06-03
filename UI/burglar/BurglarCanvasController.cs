using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Obi;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class BurglarCanvasController : MonoBehaviour {
    enum Mode { none, burglarTool, cyberdeck }
    Mode mode;
    BurgleTargetData data;
    BurglarToolType selectedTool;
    public RectTransform mainCanvas;
    // public RectTransform mainRect;
    public RawImage rawImage;
    public RectTransform camImageTransform;
    public GameObject UIElementPrefab;
    public GameObject lockIndicatorPrefab;
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
    // public 
    public WireCutterToolIndicator wireCutterImage;
    public GameObject panelButton;
    [Header("tools")]
    public GameObject keyringButton;
    public GameObject probeToolButton;
    public GameObject lockpickToolButton;
    public GameObject screwdriverToolButton;
    public GameObject keyToolButton;
    public GameObject usbToolButton;
    public GameObject wireCutterButton;

    [Header("cyber")]
    public CyberdeckCanvasController cyberdeckController;
    public RectTransform usbRectTransform;
    public GameObject usbCable;
    public bool usbCableAttached;
    public CanvasGroup usbCableCanvasGroup;
    public USBCordTool uSBCordTool;
    public AudioClip[] cyberdeckShowSound;

    [Header("sfx")]
    public AudioSource audioSource;
    public AudioClip[] pickupToolSound;
    public AudioClip[] cableRetractSound;
    public AudioClip[] cablePickupSound;
    public AudioClip[] toolOverElementSound;
    [Header("selectors")]
    public RectTransform mainPanelRect;
    public RectTransform selectorRect;
    public RectTransform burglarToolsRect;
    public RectTransform cyberdeckRect;
    public GameObject burglarSelectorObject;
    public GameObject cyberdeckSelectorObject;
    public BurglarSelectorButton[] selectorButtons;
    [Header("burglarToolBag")]
    public RectTransform burglarToolMaskRect;
    public AudioClip[] burglarBagShowSound;
    public AudioClip[] burglarBagUnzipSound;
    // TamperEvidence tamperEvidence;
    Coroutine jiggleCoroutine;
    bool mouseOverElement;
    bool mouseDown;
    AttackSurfaceElement selectedElement;
    bool finishing;
    Coroutine moveMainPanelCoroutine;
    Coroutine moveSelectorPanelCoroutine;
    Coroutine exposeBurglarToolsCoroutine;
    Coroutine exposeCyberdeckCoroutine;

    float mouseOverTimeout;

    public void Initialize(BurgleTargetData data) {
        this.data = data;
        finishing = false;

        // initialize display
        captionText.text = "";
        feedbackText.text = "";
        foreach (Transform child in uiElementsContainer) {
            if (child.name == "panelButton") continue;
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
            // Rect bounds = Toolbox.GetTotalRenderBoundingBox(element.transform, data.target.attackCam, adjustYScale: false);

            GameObject obj = GameObject.Instantiate(UIElementPrefab);
            AttackSurfaceUIElement uiElement = obj.GetComponent<AttackSurfaceUIElement>();
            obj.transform.SetParent(uiElementsContainer);
            uiElement.containerRectTransform = containerRectTransform;
            uiElement.data = data;
            uiElement.Initialize(this, element);
            element.Initialize(uiElement);
            // uiElement.ele
            uiElement.Bind(element.gameObject);
            // RectTransform elementRectTransform = obj.GetComponent<RectTransform>();

            // elementRectTransform.anchorMin = Vector2.zero;
            // elementRectTransform.anchorMax = Vector2.zero;

            // Vector3 center = Toolbox.GetBoundsCenter(element.transform);
            // Vector3 position = data.target.attackCam.WorldToViewportPoint(center);
            // position.x *= containerRectTransform.rect.width;
            // position.y *= containerRectTransform.rect.height;

            // elementRectTransform.anchoredPosition = position;
            // elementRectTransform.sizeDelta = new Vector2(bounds.width, bounds.height);

            Image cursorImage = obj.GetComponent<Image>();
            cursorImage.color = Color.red;
            cursorImage.enabled = false;

        }

        foreach (BurglarSelectorButton button in selectorButtons) {
            button.ResetPosition();
        }

        data.target.CreateTamperEvidence(data);

        panelButton.SetActive(data.target.replaceablePanel != null);
    }
    public void TearDown() {
        foreach (Transform child in uiElementsContainer) {
            if (child.name == "panelButton") continue;
            Destroy(child.gameObject);
        }
        if (data != null)
            data.target.DisableAttackSurface();
    }

    void PositionTool(Vector2 cursorPoint) {
        Vector2 localPoint = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas,
            cursorPoint, null,
            out localPoint);

        toolPoint.anchoredPosition = localPoint + new Vector2(mainCanvas.rect.width / 2f, mainCanvas.rect.height / 2f);
        // toolPoint.position = localPoint;

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

    bool ToolIsJiggly(BurglarToolType tool) {
        switch (tool) {
            case BurglarToolType.lockpick:
            case BurglarToolType.key:
            case BurglarToolType.probe:
            case BurglarToolType.screwdriver:
                return true;
            case BurglarToolType.none:
            case BurglarToolType.usb:
                return false;
            default:
                return false;
        }
    }
    void Update() {
        if (mouseOverTimeout > 0) {
            mouseOverTimeout -= Time.unscaledDeltaTime;
        }
    }
    public void UpdateWithInput(PlayerInput input) {
        mouseDown = input.mouseDown;
        PositionTool(input.mousePosition);

        // TODO: fix
        // bool outOfBounds = localPoint.x > mainRect.rect.width / 2f || localPoint.x < mainRect.rect.width / -2f || localPoint.y > mainRect.rect.height / 2f || localPoint.y < mainRect.rect.height / -2f;
        bool outOfBounds = false;

        if (input.mouseClicked && selectedTool == BurglarToolType.wirecutter) {
            wireCutterImage.DoSnip(this, data.target);
        }

        if (input.escapePressed) {
            if (selectedTool == BurglarToolType.none) {
                DoneButtonCallback();
            } else {
                SetTool(BurglarToolType.none);
            }
        } else if (input.mouseDown && outOfBounds) {
            DoneButtonCallback();
        } else if (input.mouseDown && mouseOverElement) {
            if (selectedElement != null) {
                ClickHeld(selectedElement);
            }
            if (ToolIsJiggly(selectedTool) && jiggleCoroutine == null) {
                jiggleCoroutine = StartCoroutine(JiggleTool());
            }
        } else {
            if (selectedElement != null) {
                selectedElement.HandleMouseUp();
            }
            if (jiggleCoroutine != null) {
                StopCoroutine(jiggleCoroutine);
                jiggleCoroutine = null;
            }
        }
        switch (selectedTool) {
            case BurglarToolType.lockpick:
            case BurglarToolType.probe:
            case BurglarToolType.screwdriver:
                GameManager.I.AddSuspicionRecord(SuspicionRecord.tamperingSuspicion(data));
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
            // selectedElement = null;
            SetSelectedElement(null);
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
    public void HandleAttackResult(BurglarAttackResult result) {
        if (selectedTool == BurglarToolType.usb && result.success) {
            usbCableAttached = true;
            ToolSelectCallback("none");
            // usbCable.transform.position = result.element.transform.position;
            usbRectTransform.position = result.element.uiElement.rectTransform.position;
            usbCable.transform.SetParent(transform, true);
            usbCableCanvasGroup.enabled = false;
            cyberdeckRect.transform.SetAsLastSibling();
            uSBCordTool.Slacken(true);

            // this is where we provide the cyberdeck the connected cyberware
            if (result.attachedDataStore != null)
                cyberdeckController.HandleConnection(result.attachedDataStore);
        }
        if (result != BurglarAttackResult.None) {
            AddText(result.feedbackText);
        }
        if (result.hideTamperEvidence) {
            data.target.tamperEvidence.gameObject.SetActive(false);
        } else if (result.revealTamperEvidence) {
            data.target.tamperEvidence.gameObject.SetActive(true);
        }
        if (result.makeTamperEvidenceSuspicious) {
            data.target.tamperEvidence.suspicious = true;
        }
        foreach (Vector3 lockPosition in result.lockPositions) {
            CreateLockIndicator(lockPosition);
        }
        if (result.panel != null) {
            data.target.replaceablePanel = result.panel;
            panelButton.SetActive(true);
        }
        if (result.electricDamage != null) {
            foreach (IDamageReceiver receiver in data.burglar.transform.root.GetComponentsInChildren<IDamageReceiver>()) {
                receiver.TakeDamage(result.electricDamage);
            }
            DoneButtonCallback();
        }
        if (result.finish) {
            finishing = true;
            StartCoroutine(WaitAndCloseMenu(1.5f));
        }
    }

    public void ReplacePanelButtonCallback() {
        data.target.ReplacePanel();
        panelButton.SetActive(data.target.replaceablePanel != null);
        data.target.tamperEvidence.gameObject.SetActive(false);
    }

    void CreateLockIndicator(Vector3 lockPosition) {
        GameObject lockIndicatorObject = GameObject.Instantiate(lockIndicatorPrefab);
        lockIndicatorObject.transform.SetParent(uiElementsContainer, false);
        RectTransform rectTransform = lockIndicatorObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchoredPosition = data.target.attackCam.WorldToScreenPoint(lockPosition);
        Debug.Log($"{rectTransform.anchoredPosition} {lockPosition}");
        rectTransform.sizeDelta = 100f * Vector2.one;
    }

    void AddText(string newLine) {
        feedbackText.text = feedbackText.text + $"\n{newLine}";
        string[] lines = feedbackText.text.Split('\n');
        int numLines = lines.Length;
        if (numLines > 3) {
            feedbackText.text = "";
            feedbackText.text = $"{lines[1]}\n{lines[2]}\n{lines[3]}";
        }
    }
    public void MouseOverUIElementCallback(AttackSurfaceElement element) {
        mouseOverElement = true;
        SetSelectedElement(element);
        if (selectedTool == BurglarToolType.none) {
            captionText.text = $"Use {element.elementName}";
        } else {
            if (selectedTool != BurglarToolType.usb && mouseOverTimeout <= 0) {
                mouseOverTimeout = 0.2f;
                Toolbox.RandomizeOneShot(audioSource, toolOverElementSound);
            }
            captionText.text = $"Use {selectedTool} on {element.elementName}";
        }
    }
    public void MouseExitUIElementCallback(AttackSurfaceElement element) {
        captionText.text = "";
        mouseOverElement = false;
        SetSelectedElement(null);
    }
    void SetSelectedElement(AttackSurfaceElement newElement) {
        if (newElement == selectedElement) return;
        selectedElement?.HandleFocusLost();
        selectedElement = newElement;
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
            "usb" => BurglarToolType.usb,
            "wirecutter" => BurglarToolType.wirecutter,
            _ => BurglarToolType.none
        };
        SetTool(toolType);
    }
    public AudioClip[] ToolPickupSound(BurglarToolType toolType) {
        switch (toolType) {
            default:
            case BurglarToolType.none:
                return null;
            case BurglarToolType.lockpick:
            case BurglarToolType.key:
            case BurglarToolType.probe:
            case BurglarToolType.screwdriver:
                return pickupToolSound;
            case BurglarToolType.usb:
                return cablePickupSound;
        }
    }
    void SetTool(BurglarToolType toolType) {
        // if (selectedTool != BurglarToolType.none || toolType != BurglarToolType.none)
        Toolbox.RandomizeOneShot(audioSource, ToolPickupSound(toolType));

        if (selectedTool == BurglarToolType.usb && toolType == BurglarToolType.none && !usbCableAttached) {
            Toolbox.RandomizeOneShot(audioSource, cableRetractSound);
        }

        selectedTool = toolType;
        selectedToolText.text = toolType.ToString();

        lockpickToolButton.SetActive(true);
        probeToolButton.SetActive(true);
        keyToolButton.SetActive(true);
        screwdriverToolButton.SetActive(true);
        usbToolButton.SetActive(!usbCableAttached);
        wireCutterButton.SetActive(true);
        usbCableCanvasGroup.enabled = (!usbCableAttached);

        switch (toolType) {
            case BurglarToolType.none:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                wireCutterImage.gameObject.SetActive(false);
                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.lockpick:
                probeImage.enabled = false;
                lockpickImage.enabled = true;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                lockpickToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);

                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.probe:
                probeImage.enabled = true;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                probeToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);

                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.key:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = true;
                screwdriverImage.enabled = false;
                keyToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);

                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.screwdriver:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = true;
                screwdriverToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);

                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.usb:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                wireCutterImage.gameObject.SetActive(false);


                cyberdeckController.HandleConnection(null);
                usbCableAttached = false;

                usbCable.transform.SetParent(toolPoint, true);
                // usbCable.transform.set
                usbToolButton.SetActive(false);
                usbCable.SetActive(true);
                usbCableCanvasGroup.enabled = true;
                uSBCordTool.Slacken(false);
                break;
            case BurglarToolType.wirecutter:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;

                usbCable.SetActive(usbCableAttached);
                wireCutterButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(true);
                break;
        }
    }

    IEnumerator JiggleTool() {
        while (true) {
            while (UnityEngine.Random.Range(0f, 1f) < 0.9f) {
                yield return new WaitForEndOfFrame();
            }
            Quaternion jiggle = Quaternion.AngleAxis(UnityEngine.Random.Range(-10f, 10f), toolPoint.forward);
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

    public void BurglarSelectorCallback() {
        ChangeMode(Mode.burglarTool);
    }
    public void CyberdeckSelectorCallback() {
        ChangeMode(Mode.cyberdeck);
    }
    public void BurglarSelectorCancelCallback() {
        ChangeMode(Mode.none);
    }

    void ChangeMode(Mode newMode) {
        OnModeExit(mode);
        mode = newMode;
        OnModeEnter(mode);
    }

    void OnModeExit(Mode oldMode) {
        switch (oldMode) {
            case Mode.burglarTool:
                ShowBurglarTools(false);
                break;
            case Mode.cyberdeck:
                ShowCyberDeck(false);
                break;
        }
    }
    void OnModeEnter(Mode oldMode) {
        foreach (BurglarSelectorButton button in selectorButtons) {
            button.ResetPosition();
        }
        switch (oldMode) {
            case Mode.none:
                MoveMainPanelToTop(false);
                HideSelectorPanel(false);
                ShowBurglarTools(false);
                ShowCyberDeck(false);
                burglarSelectorObject.SetActive(true);
                cyberdeckSelectorObject.SetActive(true);
                break;
            case Mode.burglarTool:
                burglarToolMaskRect.sizeDelta = new Vector2(0, 300);
                MoveMainPanelToTop(true);
                ShowBurglarTools(true);
                HideSelectorPanel(true);
                burglarSelectorObject.SetActive(false);
                cyberdeckSelectorObject.SetActive(true);
                break;
            case Mode.cyberdeck:
                MoveMainPanelToTop(true);
                ShowCyberDeck(true);
                HideSelectorPanel(true);
                burglarSelectorObject.SetActive(true);
                cyberdeckSelectorObject.SetActive(false);
                break;
        }
    }


    void MoveMainPanelToTop(bool value) {
        if (moveMainPanelCoroutine != null) {
            StopCoroutine(moveMainPanelCoroutine);
        }
        if (value) {
            moveMainPanelCoroutine = StartCoroutine(MoveRectY(mainPanelRect, -150, -20, moveMainPanelCoroutine, PennerDoubleAnimation.ExpoEaseOut));
        } else {
            moveMainPanelCoroutine = StartCoroutine(MoveRectY(mainPanelRect, -20, -150, moveMainPanelCoroutine, PennerDoubleAnimation.ExpoEaseOut));
        }
    }
    void HideSelectorPanel(bool value) {
        if (moveSelectorPanelCoroutine != null) {
            StopCoroutine(moveSelectorPanelCoroutine);
        }
        if (value) {
            moveSelectorPanelCoroutine = StartCoroutine(MoveRectY(selectorRect, 0, -100, moveSelectorPanelCoroutine, PennerDoubleAnimation.Linear));
        } else {
            moveSelectorPanelCoroutine = StartCoroutine(MoveRectY(selectorRect, -100, 0, moveSelectorPanelCoroutine, PennerDoubleAnimation.Linear));
        }
    }
    void ShowBurglarTools(bool value) {
        if (exposeBurglarToolsCoroutine != null) {
            StopCoroutine(exposeBurglarToolsCoroutine);
        }
        if (value) {
            Toolbox.RandomizeOneShot(audioSource, burglarBagShowSound);

            exposeBurglarToolsCoroutine = StartCoroutine(
                Toolbox.ChainCoroutines(MoveRectY(burglarToolsRect, -313, -40, exposeBurglarToolsCoroutine, PennerDoubleAnimation.BounceEaseOut), UnzipBurglarKit())
            );
        } else {
            exposeBurglarToolsCoroutine = StartCoroutine(MoveRectY(burglarToolsRect, -40, -313, exposeBurglarToolsCoroutine, PennerDoubleAnimation.ExpoEaseOut));
        }
    }
    void ShowCyberDeck(bool value) {
        if (exposeCyberdeckCoroutine != null) {
            StopCoroutine(exposeCyberdeckCoroutine);
        }
        if (value) {
            Toolbox.RandomizeOneShot(audioSource, cyberdeckShowSound);

            exposeCyberdeckCoroutine = StartCoroutine(MoveRectY(cyberdeckRect, -515, 170, exposeCyberdeckCoroutine, PennerDoubleAnimation.BounceEaseOut));
        } else {
            exposeCyberdeckCoroutine = StartCoroutine(MoveRectY(cyberdeckRect, 170, -515, exposeCyberdeckCoroutine, PennerDoubleAnimation.ExpoEaseOut));
        }
    }
    IEnumerator UnzipBurglarKit() {
        RectTransform rect = burglarToolMaskRect;
        Toolbox.RandomizeOneShot(audioSource, burglarBagUnzipSound);
        float timer = 0;
        float duration = 0.3f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float x = (float)PennerDoubleAnimation.Linear(timer, 0, 1256, duration);
            rect.sizeDelta = new Vector2(x, 300f);
            yield return null;
        }
        rect.sizeDelta = new Vector2(1256, 300);
    }
    IEnumerator MoveRectY(RectTransform rect, float startY, float endY, Coroutine routine, Func<double, double, double, double, double> easing) {
        float timer = 0;
        float duration = 0.5f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float y = (float)easing(timer, startY, endY - startY, duration);
            Vector2 newPosition = new Vector3(rect.anchoredPosition.x, y);
            rect.anchoredPosition = newPosition;
            yield return null;
        }
        Vector2 finalPosition = new Vector3(rect.anchoredPosition.x, endY);
        rect.anchoredPosition = finalPosition;
        routine = null;
    }
}
