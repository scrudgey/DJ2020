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
    public Image keyCardImage;
    // public 
    public WireCutterToolIndicator wireCutterImage;
    [Header("buttons")]
    public GameObject panelButton;
    public GameObject returnCameraButton;
    [Header("tools")]
    public GameObject keyringButton;
    public GameObject probeToolButton;
    public GameObject lockpickToolButton;
    public GameObject screwdriverToolButton;
    public GameObject keyToolButton;
    public GameObject usbToolButton;
    public GameObject wireCutterButton;
    public GameObject keycardButton;

    [Header("cyber")]
    // public CyberdeckCanvasController cyberdeckController;
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


    Camera currentAttackCamera;
    public void Initialize(BurgleTargetData data) {
        this.data = data;
        SetCamera(data.target.attackCam);

        finishing = false;

        if (data.target.obiSolver != null) {
            data.target.obiSolver.enabled = true;
        }

        // initialize display
        captionText.text = "";
        feedbackText.text = "";
        foreach (Transform child in uiElementsContainer) {
            if (child.name == "panelButton") continue;
            if (child.gameObject == returnCameraButton) continue;
            Destroy(child.gameObject);
        }
        SetTool(BurglarToolType.none);
        keyringButton.SetActive(GameManager.I.gameData.levelState.delta.physicalKeys.Count > 0);

        // configure elements
        data.target.EnableAttackSurface();
        rawImage.texture = data.target.renderTexture;
        rawImage.color = Color.white;
        RectTransform containerRectTransform = uiElementsContainer.GetComponent<RectTransform>();
        foreach (AttackSurfaceElement element in data.target.attackElementRoot.GetComponentsInChildren<AttackSurfaceElement>(true)) {
            GameObject obj = GameObject.Instantiate(UIElementPrefab);
            AttackSurfaceUIElement uiElement = obj.GetComponent<AttackSurfaceUIElement>();
            obj.transform.SetParent(uiElementsContainer);
            uiElement.containerRectTransform = containerRectTransform;
            uiElement.data = data;
            uiElement.Initialize(this, element);
            element.Initialize(uiElement);
            uiElement.Bind(element.gameObject);
            Image cursorImage = obj.GetComponent<Image>();
            cursorImage.color = Color.red;
            SpriteRenderer spriteRenderer = element.GetComponentInChildren<SpriteRenderer>();
            if (element.buttonSprite != null) {
                foreach (Image image in uiElement.buttonImages) {
                    image.sprite = element.buttonSprite;
                    // image.alphaHitTestMinimumThreshold = 0.5f;
                    image.type = Image.Type.Simple;
                    image.color = Color.clear;
                }
            } else {
                foreach (Image image in uiElement.buttonImages) {
                    image.type = Image.Type.Simple;
                    image.color = Color.clear;
                }
            }
            cursorImage.enabled = false;
        }

        foreach (BurglarSelectorButton button in selectorButtons) {
            button.ResetPosition();
        }

        data.target.CreateTamperEvidence(data);
        returnCameraButton.SetActive(false);
        panelButton.SetActive(data.target.replaceablePanel != null);
    }
    public void TearDown() {
        foreach (Transform child in uiElementsContainer) {
            if (child.name == "panelButton") continue;
            if (child.gameObject == returnCameraButton) continue;
            Destroy(child.gameObject);
        }
        if (data != null)
            data.target.DisableAttackSurface();
    }

    bool PositionTool(Vector2 cursorPoint) {
        Vector2 localPoint = Vector2.zero;
        Vector2 localToolPoint = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiElementsRectTransform,
            cursorPoint, null,
            out localPoint);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas,
            cursorPoint, null,
            out localToolPoint);


        toolPoint.anchoredPosition = localToolPoint + new Vector2(mainCanvas.rect.width / 2f, mainCanvas.rect.height / 2f);

        if (localPoint.x > uiElementsRectTransform.rect.width / -2f && localPoint.x < uiElementsRectTransform.rect.width / 2f &&
            localPoint.y > uiElementsRectTransform.rect.height / -2f && localPoint.y < uiElementsRectTransform.rect.height / 2f) {
            if (selectedTool == BurglarToolType.none) {
                GameManager.I.cursorType = CursorType.hand;
            } else {
                GameManager.I.cursorType = CursorType.gun;
            }
            return false;
        } else {
            GameManager.I.cursorType = CursorType.pointer;
            return true;
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

    public Ray MousePositionToAttackCamRay(Vector2 mousePosition) {
        // Vector3 cursorPoint = input.mousePosition;
        Vector3 cursorPoint = mousePosition;

        float pixelToCanvasRatio = Screen.height / mainCanvas.rect.height;

        // Debug.Log($"cursorpoint original: {cursorPoint}\nmouseposition: {input.mousePosition}\ninput viewportpoint; {input.viewPortPoint}");
        // Debug.Log($"main camera: {Camera.main.pixelWidth} x {Camera.main.pixelHeight}");
        // Debug.Log($"screen: {Screen.width} x {Screen.height}");
        // Debug.Log($"attack cam: {data.target.attackCam.pixelWidth} x {data.target.attackCam.pixelHeight}");
        // Debug.Log($"main canvas: {mainCanvas.rect.width} x {mainCanvas.rect.height}");

        cursorPoint -= camImageTransform.position;

        // float rectWidth = (uiElementsRectTransform.anchorMax.x - uiElementsRectTransform.anchorMin.x) * Screen.width;
        // float rectHeight = (uiElementsRectTransform.anchorMax.y - uiElementsRectTransform.anchorMin.y) * Screen.height;

        float rectWidth = (uiElementsRectTransform.anchorMax.x - uiElementsRectTransform.anchorMin.x) * currentAttackCamera.pixelWidth * pixelToCanvasRatio;
        float rectHeight = (uiElementsRectTransform.anchorMax.y - uiElementsRectTransform.anchorMin.y) * currentAttackCamera.pixelHeight * pixelToCanvasRatio;

        Vector3 viewPortPoint = new Vector3(cursorPoint.x / rectWidth, cursorPoint.y / rectHeight, currentAttackCamera.nearClipPlane);
        // Debug.Log($"camImageTransform position: {camImageTransform.position}\ncursorpoint translated: {cursorPoint}\nnew viewportpoint: {viewPortPoint}\n rect: {rectWidth} x {rectHeight}");

        Ray projection = currentAttackCamera.ViewportPointToRay(viewPortPoint);

        return projection;
    }

    void SetCamera(Camera camera) {
        if (currentAttackCamera != null) {
            currentAttackCamera.enabled = false;
        }
        currentAttackCamera = camera;
        currentAttackCamera.enabled = true;
        rawImage.texture = camera.targetTexture;
    }

    void RaycastToolPosition(Vector2 mousePosition) {
        Ray projection = MousePositionToAttackCamRay(mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(projection, 1000, LayerUtil.GetLayerMask(Layer.def, Layer.interactive, Layer.attackSurface, Layer.bulletOnly, Layer.skyboxNoLight));
        bool noHitFound = true;
        foreach (RaycastHit hit in hits.OrderBy(hit => hit.distance)) {
            AttackSurfaceElement element = hit.collider.GetComponentInChildren<AttackSurfaceElement>();
            if (element != null) {
                noHitFound = false;
                if (element != null && element != selectedElement) {
                    noHitFound = false;
                    // Debug.Log($"happening: {element} {selectedElement} ");
                    if (selectedElement != null) {
                        MouseExitUIElementCallback(selectedElement);
                    }
                    MouseOverUIElementCallback(element);
                }
                break;
            }

        }
        if (noHitFound && selectedElement != null) {
            MouseExitUIElementCallback(selectedElement);
        }
        if (hits.Length == 0 && selectedElement != null) {
            MouseExitUIElementCallback(selectedElement);
        }
        // key is to find AttackSurfaceElement
    }
    public void UpdateWithInput(PlayerInput input) {
        bool outOfBounds = PositionTool(input.mousePosition);
        if (data != null && data.target != null && !outOfBounds)
            RaycastToolPosition(input.mousePosition);

        bool newMouseDown = input.mouseDown;
        if (newMouseDown != mouseDown && newMouseDown && !outOfBounds) {
            ClickDown(selectedElement);
        }
        mouseDown = newMouseDown;

        if (input.mouseClicked && selectedTool == BurglarToolType.wirecutter && !outOfBounds) {
            wireCutterImage.DoSnip(this, data.target, input.mousePosition);
        }

        if (input.escapePressed) {
            if (selectedTool == BurglarToolType.none) {
                DoneButtonCallback();
            } else {
                SetTool(BurglarToolType.none);
            }
        } else if (input.mouseDown && outOfBounds) {
            SetTool(BurglarToolType.none);
        } else if (input.mouseDown && mouseOverElement) {
            if (selectedElement != null) {
                ClickHeld(selectedElement);
            }
            if (ToolIsJiggly(selectedTool) && jiggleCoroutine == null && !selectedElement.resetToolJiggle) {
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

        if (jiggleCoroutine != null && (selectedElement?.resetToolJiggle ?? false)) {
            StopCoroutine(jiggleCoroutine);
            jiggleCoroutine = null;
        }
        switch (selectedTool) {
            case BurglarToolType.lockpick:
            case BurglarToolType.probe:
            case BurglarToolType.screwdriver:
            case BurglarToolType.wirecutter:
                GameManager.I.AddSuspicionRecord(SuspicionRecord.tamperingSuspicion(data));
                break;
        }
    }
    public void DoneButtonCallback() {

        CloseBurglar();
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
    public void ClickDown(AttackSurfaceElement element) {
        if (element == null || finishing)
            return;
        BurglarAttackResult result = element.HandleSingleClick(selectedTool, data);
        HandleAttackResult(result);
    }

    public void HandleAttackResult(BurglarAttackResult result) {
        if (selectedTool == BurglarToolType.usb && result.success) { // TODO
            usbCableAttached = true;
            SetTool(BurglarToolType.none);
            usbRectTransform.position = result.element.uiElement.rectTransform.position;
            usbCable.transform.SetParent(transform, true);
            usbCableCanvasGroup.enabled = false;
            cyberdeckRect.transform.SetAsLastSibling();
            uSBCordTool.Slacken(true);
            Rect bounds = Toolbox.GetTotalRenderBoundingBox(result.element.transform, currentAttackCamera);
            uSBCordTool.SetSize(bounds.width / 1.8f);
            // cyberdeckController.HandleConnection(result);
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

        if (result.tamperEvidenceReportString != "") {
            data.target.tamperEvidence.reportText = result.tamperEvidenceReportString;
        }

        if (result.lockPositions != null)
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
            CloseBurglar(transitionCharacter: false);
        }

        if (result.changeCamera != null) {
            SetCamera(result.changeCamera);
            ShowReturnButton();
        }

        if (result.finish) {
            finishing = true;
            StartCoroutine(WaitAndCloseMenu(1.5f));
        }
    }

    void ShowReturnButton() {
        returnCameraButton.SetActive(true);
    }
    public void ReturnButtonCallback() {
        SetCamera(data.target.attackCam);
        returnCameraButton.SetActive(false);
    }

    void CloseBurglar(bool transitionCharacter = true) {
        ResetUSBTool();
        // cyberdeckController.CancelHackInProgress();
        if (data.target.obiSolver != null) {
            data.target.obiSolver.enabled = false;
        }
        GameManager.I.CloseBurglar(transitionCharacter: transitionCharacter);
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
        rectTransform.anchoredPosition = currentAttackCamera.WorldToScreenPoint(lockPosition);
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
        if (selectedElement != element && selectedTool != BurglarToolType.usb && selectedTool != BurglarToolType.none && mouseOverTimeout <= 0) {
            mouseOverTimeout = 0.75f;
            Toolbox.RandomizeOneShot(audioSource, toolOverElementSound);
        }
        SetSelectedElement(element);
        if (selectedTool == BurglarToolType.none) {
            captionText.text = $"Use {element.elementName}";
        } else {
            captionText.text = $"Use {selectedTool} on {element.elementName}";
        }
        element.OnMouseOver();
    }
    public void MouseExitUIElementCallback(AttackSurfaceElement element) {
        captionText.text = "";
        mouseOverElement = false;
        SetSelectedElement(null);
        if (element != null)
            element.OnMouseExit();
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
            "keycard" => BurglarToolType.keycard,
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
        toolPoint.rotation = Quaternion.identity;

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
        keycardButton.SetActive(true);
        usbCableCanvasGroup.enabled = (!usbCableAttached);

        switch (toolType) {
            case BurglarToolType.none:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                wireCutterImage.gameObject.SetActive(false);
                keyCardImage.enabled = false;
                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.lockpick:
                probeImage.enabled = false;
                lockpickImage.enabled = true;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                keyCardImage.enabled = false;

                lockpickToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);

                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.probe:
                probeImage.enabled = true;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                keyCardImage.enabled = false;

                probeToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);

                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.key:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = true;
                screwdriverImage.enabled = false;
                keyCardImage.enabled = false;

                keyToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);

                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.screwdriver:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = true;
                keyCardImage.enabled = false;
                screwdriverToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);
                usbCable.SetActive(usbCableAttached);
                break;
            case BurglarToolType.usb:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                keyCardImage.enabled = false;

                wireCutterImage.gameObject.SetActive(false);

                ResetUSBTool();

                usbCable.transform.SetParent(toolPoint, true);
                usbCable.SetActive(true);
                usbCableCanvasGroup.enabled = true;
                break;
            case BurglarToolType.wirecutter:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                keyCardImage.enabled = false;
                screwdriverImage.enabled = false;

                usbCable.SetActive(usbCableAttached);
                wireCutterButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(true);
                break;
            case BurglarToolType.keycard:
                probeImage.enabled = false;
                lockpickImage.enabled = false;
                keyImage.enabled = false;
                screwdriverImage.enabled = false;
                keyCardImage.enabled = true;
                screwdriverToolButton.SetActive(false);
                wireCutterImage.gameObject.SetActive(false);
                usbCable.SetActive(usbCableAttached);
                keycardButton.SetActive(false);
                break;
        }
    }

    void ResetUSBTool() {
        usbCable.transform.SetParent(toolPoint, false);
        usbCable.transform.localPosition = Vector2.zero;
        // cyberdeckController.HandleConnection(null);
        usbCableAttached = false;
        usbToolButton.SetActive(false);
        uSBCordTool.Slacken(false);
        uSBCordTool.SetSize(100f);
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
        ResetUSBTool();
        SetTool(BurglarToolType.none);
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
            exposeBurglarToolsCoroutine = StartCoroutine(MoveRectY(burglarToolsRect, burglarToolsRect.anchoredPosition.y, -313, exposeBurglarToolsCoroutine, PennerDoubleAnimation.ExpoEaseOut));
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
            exposeCyberdeckCoroutine = StartCoroutine(MoveRectY(cyberdeckRect, cyberdeckRect.anchoredPosition.y, -515, exposeCyberdeckCoroutine, PennerDoubleAnimation.ExpoEaseOut));
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
