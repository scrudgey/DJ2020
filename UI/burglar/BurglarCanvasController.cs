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
    bool preventCloseButton;
    bool preventButtons;
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
    Coroutine selectedToolTextRoutine;
    Coroutine captionRoutine;
    Coroutine elementHighlightTextRoutine;
    public Color feedbackColor;
    public Image elementHighlight;
    public RectTransform elementHighlightRectTransform;
    public TextMeshProUGUI elementHighlightText;

    // public TextMeshProUGUI feedbackText;
    public TerminalAnimation feedback;
    public RectTransform toolPoint;
    public Image probeImage;
    public Image lockpickImage;
    public Image keyImage;
    public Image screwdriverImage;
    public Image keyCardImage;
    // public 
    public WireCutterToolIndicator wireCutterImage;
    [Header("buttons")]
    public GameObject returnCameraButton;
    public GameObject handToolButton;
    public GameObject closeButton;
    [Header("tools")]
    public GameObject keyringButton;
    public GameObject probeToolButton;
    public GameObject lockpickToolButton;
    public GameObject screwdriverToolButton;
    public GameObject keyToolButton;
    public GameObject usbToolButton;
    public GameObject wireCutterButton;
    public GameObject keycardButton;

    [Header("usb")]
    // public CyberdeckCanvasController cyberdeckController;
    public MultiToolController multiToolController;
    public GameObject usbCable;
    public bool usbCableAttached;
    // public CanvasGroup usbCableCanvasGroup;
    public USBCordTool uSBCordTool;
    // public AudioClip[] cyberdeckShowSound;

    [Header("sfx")]
    public AudioSource audioSource;
    public AudioClip[] pickupToolSound;
    public AudioClip[] cableRetractSound;
    public AudioClip[] cablePickupSound;
    public AudioClip[] toolOverElementSound;
    [Header("selectors")]
    // public RectTransform mainPanelRect;
    public RectTransform selectorRect;
    public RectTransform burglarToolsRect;
    public RectTransform cyberdeckRect;
    public bool burglarToolsSelected;
    // public GameObject burglarSelectorObject;
    // public GameObject cyberdeckSelectorObject;
    // public BurglarSelectorButton[] selectorButtons;
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
    // Coroutine moveMainPanelCoroutine;
    // Coroutine moveSelectorPanelCoroutine;
    // Coroutine exposeBurglarToolsCoroutine;
    Coroutine exposeCyberdeckCoroutine;

    float mouseOverTimeout;
    float rejectClickTimeout;

    Camera currentAttackCamera;
    Coroutine tutorialDialogueCoroutine;
    public void Initialize(BurgleTargetData data) {
        this.data = data;
        SetCamera(data.target.attackCam);

        finishing = false;

        if (data.target.obiSolver != null) {
            data.target.obiSolver.enabled = true;
        }

        // initialize display
        captionText.text = "";
        elementHighlight.enabled = false;
        elementHighlightText.text = "";
        // feedbackText.text = "";
        feedback.Clear();
        foreach (Transform child in uiElementsContainer) {
            if (child.name == "panelButton") continue;
            if (child.gameObject == returnCameraButton) continue;
            if (child.gameObject == handToolButton) continue;
            if (child.gameObject == closeButton) continue;
            if (child.gameObject == elementHighlight.gameObject) continue;
            Destroy(child.gameObject);
        }
        SetTool(BurglarToolType.none);
        keyringButton.SetActive(GameManager.I.gameData.levelState.delta.keys.Where(key => key.type == KeyType.physical).Count() > 0);

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

        // foreach (BurglarSelectorButton button in selectorButtons) {
        //     button.ResetPosition();
        // }

        data.target.CreateTamperEvidence(data);
        returnCameraButton.SetActive(false);
        if (!burglarToolsSelected) {
            BurglarSelectorCallback();
        }
    }
    public void TearDown() {
        foreach (Transform child in uiElementsContainer) {
            if (child.name == "panelButton") continue;
            if (child.gameObject == returnCameraButton) continue;
            if (child.gameObject == handToolButton) continue;
            if (child.gameObject == closeButton) continue;
            if (child.gameObject == elementHighlight.gameObject) continue;
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
        if (rejectClickTimeout > 0) {
            rejectClickTimeout -= Time.unscaledDeltaTime;
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
                ToolSelectCallback("none");
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
        if (!preventCloseButton)
            CloseBurglar();
    }

    public void ClickHeld(AttackSurfaceElement element) {
        if (finishing)
            return;
        BurglarAttackResult result = element.HandleClickHeld(selectedTool, data);
        if (result.success) {
            mouseOverElement = false;
            SetSelectedElement(null);
        }
        HandleAttackResult(result);
    }
    public void ClickDown(AttackSurfaceElement element) {
        if (element == null || finishing)
            return;
        data.camera = currentAttackCamera;
        BurglarAttackResult result = element.HandleSingleClick(selectedTool, data);
        HandleAttackResult(result);
    }

    public void HandleAttackResult(BurglarAttackResult result) {
        if (selectedTool == BurglarToolType.usb && result.success) { // TODO
            rejectClickTimeout = 0.2f;
            usbCableAttached = true;
            SetTool(BurglarToolType.none);
            // usbRectTransform.position = result.element.uiElement.rectTransform.position;
            usbCable.transform.SetParent(transform, true);
            // usbCableCanvasGroup.enabled = false;
            cyberdeckRect.transform.SetAsLastSibling();
            uSBCordTool.Slacken(true);
            // Rect bounds = Toolbox.GetTotalRenderBoundingBox(result.element.transform, currentAttackCamera);
            // uSBCordTool.SetSize(bounds.width / 1.8f);
            // cyberdeckController.HandleConnection(result);
            multiToolController.HandleConnection(result);
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

        if (result.electricDamage != null) {
            foreach (IDamageReceiver receiver in data.burglar.transform.root.GetComponentsInChildren<IDamageReceiver>()) {
                receiver.TakeDamage(result.electricDamage);
            }
            CloseBurglar(transitionCharacter: false);
        }

        if (result.changeCamera != null) {
            if (result.changeCameraQuad != null) {
                StartCoroutine(zoomIntoCamera(result.changeCameraQuad, result.changeCamera));
                elementHighlight.enabled = false;
                elementHighlightText.enabled = false;
            } else {
                SetCamera(result.changeCamera);
                ShowReturnButton(true);
                ShowCyberDeck(true);
            }
        }

        if (result.finish) {
            finishing = true;
            StartCoroutine(WaitAndCloseMenu(1.5f));
        }

        if (result.activateHVACNetwork) {
            GameManager.I.CloseBurglar();
            GameManager.I.playerCharacterController.ActivateHVAC(result.HVACNetwork, result.HVACStartElement);
        }
    }

    IEnumerator zoomIntoCamera(Renderer quad, Camera changeCamera) {
        quad.enabled = true;
        changeCamera.enabled = true;



        // camera = UnityEngine.Camera.main;
        // halfViewport = (changeCamera.orthographicSize * changeCamera.aspect);
        // Vector3 viewPos = changeCamera.WorldToViewportPoint( )

        Vector3 displacement = quad.transform.position - currentAttackCamera.transform.position;
        Vector3 forwardDisplacement = Vector3.Project(displacement, currentAttackCamera.transform.forward);

        Vector3 initialQuadPosition = quad.transform.position;
        Vector3 finalQuadPosition = currentAttackCamera.transform.position + forwardDisplacement;

        Quaternion targetRotation = currentAttackCamera.transform.rotation;
        Quaternion initialRotation = quad.transform.rotation;

        Vector3 initialScale = quad.transform.localScale;

        /*
                   /  |
                /     | h/2
             /θ/2     |
            *----------
                d
        tan θ = h / 2d
        h = 2 d tan θ/2
         */


        yield return Toolbox.Ease(null, 0.5f, 0f, 1f, PennerDoubleAnimation.CubicEaseOut, (amount) => {
            quad.transform.position = Vector3.Lerp(initialQuadPosition, finalQuadPosition, amount);
            quad.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, amount);
        });

        Transform parent = quad.transform.parent;
        quad.transform.SetParent(null, true);

        // change position and scale simultaneously to bring it closer to camera
        float distance = forwardDisplacement.magnitude;
        float initialAngle = Mathf.Atan(quad.transform.localScale.y / distance);

        float nearDistance = 0.35f;
        float rescaleStart = 2f * nearDistance * Mathf.Tan(initialAngle);
        float rescaleFinish = 2f * nearDistance * Mathf.Tan(Mathf.Deg2Rad * currentAttackCamera.fieldOfView / 2f);

        Vector3 nearPosition = currentAttackCamera.transform.position + (forwardDisplacement.normalized * nearDistance);

        quad.transform.position = nearPosition;
        quad.transform.localScale = new Vector3(1.6f * rescaleStart, rescaleStart, 1f);

        yield return Toolbox.Ease(null, 0.5f, rescaleStart, rescaleFinish, PennerDoubleAnimation.CubicEaseOut, (amount) => {
            quad.transform.localScale = new Vector3(1.6f * amount, amount, 1f);
        });

        quad.transform.SetParent(parent, true);

        quad.enabled = false;
        quad.transform.position = initialQuadPosition;
        quad.transform.rotation = initialRotation;
        quad.transform.localScale = initialScale;

        SetCamera(changeCamera);
        ShowReturnButton(true);
        ShowCyberDeck(true);

        CutsceneManager.I.HandleTrigger("circuit_view");
    }

    public void ShowReturnButton(bool value) {
        returnCameraButton.SetActive(value);
    }
    public void ReturnButtonCallback() {
        if (selectedTool == BurglarToolType.usb) {
            SetTool(BurglarToolType.none);
        }
        SetCamera(data.target.attackCam);
        returnCameraButton.SetActive(false);
        ShowCyberDeck(false);
    }

    void CloseBurglar(bool transitionCharacter = true) {
        ShowCyberDeck(false);
        ResetUSBTool();
        // cyberdeckController.CancelHackInProgress();
        if (data.target.obiSolver != null) {
            data.target.obiSolver.enabled = false;
        }
        GameManager.I.CloseBurglar(transitionCharacter: transitionCharacter);
    }

    // public void ReplacePanelButtonCallback() {
    //     data.target.ReplacePanel();
    //     panelButton.SetActive(data.target.replaceablePanel != null);
    //     data.target.tamperEvidence.gameObject.SetActive(false);
    // }

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
        // feedbackText.text = feedbackText.text + $"\n{newLine}";
        // string[] lines = feedbackText.text.Split('\n');
        // int numLines = lines.Length;
        // if (numLines > 3) {
        //     feedbackText.text = "";
        //     feedbackText.text = $"{lines[1]}\n{lines[2]}\n{lines[3]}";
        // } 
        // feedback.
        Writeln[] writes = new Writeln[1]{
            new Writeln("", $"{newLine}", feedbackColor) {
                destroyAfter = 5f,
            }
        };

        feedback.DoWriteMany(writes);
    }
    public void MouseOverUIElementCallback(AttackSurfaceElement element) {
        if (element == null || element.uiElement == null) return;
        mouseOverElement = true;
        if (selectedElement != element && selectedTool != BurglarToolType.usb && selectedTool != BurglarToolType.none && mouseOverTimeout <= 0) {
            mouseOverTimeout = 0.75f;
            Toolbox.RandomizeOneShot(audioSource, toolOverElementSound);
        }
        SetSelectedElement(element);
        if (selectedTool == BurglarToolType.usb) {
            multiToolController.MouseOverUIElementCallback(element);
            uSBCordTool.SetSpriteOpen(false);
        } else if (selectedTool == BurglarToolType.none) {
            SetCaptionText($"Use {element.elementName}");
        } else {
            SetCaptionText($"Use {selectedTool} on {element.elementName}");
        }
        elementHighlight.enabled = true;
        SetHighlightText(element.elementName);
        elementHighlightRectTransform.position = element.uiElement.rectTransform.position;
        Rect bounds = Toolbox.GetTotalRenderBoundingBox(element.transform, currentAttackCamera, adjustYScale: false, useColliders: true);
        elementHighlightRectTransform.sizeDelta = new Vector2(bounds.width, bounds.height);
        element.OnMouseOver();
    }
    void SetCaptionText(string content) {
        if (captionRoutine != null) {
            StopCoroutine(captionRoutine);
        }
        // captionRoutine = StartCoroutine(Toolbox.BlitText(captionText, content));
        captionText.text = content;
    }
    void SetHighlightText(string content) {
        if (elementHighlightTextRoutine != null) {
            StopCoroutine(elementHighlightTextRoutine);
        }
        elementHighlightTextRoutine = StartCoroutine(Toolbox.BlitText(elementHighlightText, content));
    }
    public void MouseExitUIElementCallback(AttackSurfaceElement element) {
        captionText.text = "";
        mouseOverElement = false;
        elementHighlight.enabled = false;
        elementHighlightText.text = "";
        SetSelectedElement(null);
        if (selectedTool == BurglarToolType.usb) {
            multiToolController.MouseExitUIElementCallback(element);
            uSBCordTool.SetSpriteOpen(true);
        }
        if (element != null)
            element.OnMouseExit();
    }
    void SetSelectedElement(AttackSurfaceElement newElement) {
        if (newElement == selectedElement) return;
        selectedElement?.HandleFocusLost();
        selectedElement = newElement;
    }
    public void MouseEnterToolButton(string toolName) {
        if (selectedToolTextRoutine != null) {
            StopCoroutine(selectedToolTextRoutine);
        }
        if (preventButtons) {
            return;
        }
        if (toolName.ToLower().Contains("none")) {
            selectedToolText.text = "";
        } else {
            selectedToolTextRoutine = StartCoroutine(Toolbox.BlitText(selectedToolText, toolName));
        }
    }
    public void MouseExitToolButton() {
        if (selectedToolTextRoutine != null) {
            StopCoroutine(selectedToolTextRoutine);
        }
        if (preventButtons) {
            return;
        }
        // selectedToolText.text = selectedTool.ToString();
        selectedToolText.text = "";
    }

    public void HandToolButtonCallback() {
        if (preventButtons) {
            return;
        }
        ToolSelectCallback("none");
    }

    public void ToolSelectCallback(string toolName) {
        InputProfile inputProfile = CutsceneManager.I.runningCutscene()?.inputProfile ?? InputProfile.allowAll;
        if (!inputProfile.allowBurglarInterface && toolName != "none") {
            return;
        }

        if (rejectClickTimeout > 0) return;
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
        multiToolController.OnToolSelect(toolType);
        rejectClickTimeout = 0.2f;
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
        if (tutorialDialogueCoroutine != null) {
            GameManager.I.uiController.HideCutsceneDialogue();
            StopCoroutine(tutorialDialogueCoroutine);
        }
        Toolbox.RandomizeOneShot(audioSource, ToolPickupSound(toolType));
        toolPoint.rotation = Quaternion.identity;

        if (selectedTool == BurglarToolType.usb && toolType == BurglarToolType.none && !usbCableAttached) {
            Toolbox.RandomizeOneShot(audioSource, cableRetractSound);
        }

        selectedTool = toolType;
        // selectedToolText.text = toolType.ToString();
        if (selectedToolTextRoutine != null) {
            StopCoroutine(selectedToolTextRoutine);
        }
        selectedToolText.text = "";

        lockpickToolButton.SetActive(true);
        probeToolButton.SetActive(true);
        keyToolButton.SetActive(true);
        screwdriverToolButton.SetActive(true);
        usbToolButton.SetActive(!usbCableAttached);
        wireCutterButton.SetActive(true);
        keycardButton.SetActive(true);
        // usbCableCanvasGroup.enabled = (!usbCableAttached);

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
                uSBCordTool.SetSpriteOpen(true);

                usbCable.transform.SetParent(toolPoint, true);
                usbCable.SetActive(true);
                // usbCableCanvasGroup.enabled = true;
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
        handToolButton.SetActive(selectedTool != BurglarToolType.none);
        CutsceneManager.I.HandleTrigger($"burglartool_{toolType}");
    }

    void ResetUSBTool() {
        usbCable.transform.SetParent(toolPoint, false);
        usbCable.transform.localPosition = Vector2.zero;
        // cyberdeckController.HandleConnection(null);
        usbCableAttached = false;
        usbToolButton.SetActive(false);
        uSBCordTool.Slacken(false);
        uSBCordTool.SetSize(100f);
        multiToolController.OnUSBToolReset();
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
        burglarToolMaskRect.sizeDelta = new Vector2(0, 300);
        ShowBurglarTools(true);
        StartCoroutine(MoveRectY(selectorRect, 106, -100, PennerDoubleAnimation.Linear, duration: 0.1f));
    }
    void ShowBurglarTools(bool value) {
        burglarToolsSelected = value;
        if (value) {
            Toolbox.RandomizeOneShot(audioSource, burglarBagShowSound);
            StartCoroutine(
               Toolbox.ChainCoroutines(MoveRectY(burglarToolsRect, -313, -40, PennerDoubleAnimation.BounceEaseOut), UnzipBurglarKit())
           );
        } else {
            StartCoroutine(MoveRectY(burglarToolsRect, burglarToolsRect.anchoredPosition.y, -313, PennerDoubleAnimation.ExpoEaseOut));
        }
    }
    void ShowCyberDeck(bool value) {
        if (exposeCyberdeckCoroutine != null) {
            StopCoroutine(exposeCyberdeckCoroutine);
        }
        if (value) {
            // Toolbox.RandomizeOneShot(audioSource, cyberdeckShowSound);
            exposeCyberdeckCoroutine = StartCoroutine(MoveRectY(cyberdeckRect, -600, 127, PennerDoubleAnimation.BounceEaseOut));
            multiToolController.Initialize();
        } else {
            exposeCyberdeckCoroutine = StartCoroutine(MoveRectY(cyberdeckRect, cyberdeckRect.anchoredPosition.y, -600, PennerDoubleAnimation.ExpoEaseOut));
            ResetUSBTool();
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
    IEnumerator MoveRectY(RectTransform rect, float startY, float endY, Func<double, double, double, double, double> easing, float duration = 0.5f) {
        float timer = 0;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float y = (float)easing(timer, startY, endY - startY, duration);
            Vector2 newPosition = new Vector3(rect.anchoredPosition.x, y);
            rect.anchoredPosition = newPosition;
            yield return null;
        }
        Vector2 finalPosition = new Vector3(rect.anchoredPosition.x, endY);
        rect.anchoredPosition = finalPosition;
    }

    public void PreventClose(bool value) {
        preventCloseButton = value;
        closeButton.SetActive(!value);
    }
    public void PreventButtons(bool value) {
        preventButtons = value;
        if (selectedToolTextRoutine != null) {
            StopCoroutine(selectedToolTextRoutine);
        }
        selectedToolText.text = "";
    }
}
