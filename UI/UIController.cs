using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class UIController : MonoBehaviour {
    [Header("canvas")]
    public Canvas canvas;
    public Canvas gunDisplayCanvas;
    public Canvas terminalCanvas;
    public Canvas burglarCanvas;
    // public Canvas missionSelectorCanvas;
    public Canvas interactiveHighlightCanvas;
    public Canvas statusBarCanvas;
    public Canvas overlayCanvas;
    public Canvas cutsceneTextCanvas;
    public Canvas cutsceneDialogueCanvas;
    public Canvas indicatorCanvas;
    // public Canvas 
    [Header("controllers")]
    public TerminalController terminal;
    public WeaponUIHandler weaponUIHandler;
    public MeleeWeaponUIHandler meleeWeaponUIHandler;
    public ItemUIHandler itemUIHandler;
    public AimIndicatorHandler aimIndicatorHandler;
    public LockRadiusIndicatorHandler lockRadiusIndicatorHandler;
    public LockIndicatorHandler lockIndicatorHandler;
    public InteractiveHighlightHandler interactiveHighlightHandler;
    public ActionLogHandler actionLogHandler;
    public OverlayHandler overlayHandler;
    public GameObject UIEditorCamera;
    // public HackDisplay hackDisplay;
    public VisibilityUIHandler visibilityUIHandler;
    public SuspicionIndicatorHandler suspicionIndicatorHandler;
    public HealthIndicatorController healthIndicatorController;
    public PlayerCalloutHandler playerCalloutHandler;
    public PlayerArrowCalloutHandler playerArrowCalloutHandler;
    public TextMeshProUGUI caption;
    public HitIndicatorController hitIndicatorController;
    public VRStatHandler vRStatHandler;
    public BurglarCanvasController burglarCanvasController;
    public ObjectiveCanvasController objectiveCanvasController;
    public ObjectivesCompleteController objectivesCompleteController;
    // public MissionComputerController missionComputerController;
    public SaveIndicatorController saveIndicatorController;
    public WeaponWheelController weaponWheelController;
    public TargetPracticeUIHandler targetPracticeUIHandler;
    public UIStatusBarHandler statusBarHandler;
    public SoftwareModalController softwareModalController;
    public KeySelectMenu keySelectMenu;
    public bool mouseOverScrollBox;
    public IndicatorUIController indicatorUIController;
    [Header("cutscene stuff")]
    public TextMeshProUGUI creditsText;

    public TextMeshProUGUI cutsceneText;
    public TextMeshProUGUI tutorialText;
    public CutsceneDialogueController cutsceneDialogueController;
    public CanvasGroup burglarCanvasGroup;
    public RectTransform overlayButtonHighlight;
    public CanvasGroup overlayButtonGroup;
    public bool cutsceneDialogueEnabled;
    Coroutine overlayButtonHighlightRoutine;
    bool burglarMode;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize() {
        Debug.Log("UI Initialize");
        // cameras
        canvas.worldCamera = Camera.main;
        interactiveHighlightHandler.cam = Camera.main;
        overlayHandler.uIController = this;
        overlayHandler.cam = Camera.main;
        overlayHandler.ClearAllIndicatorsAndEdges();
        aimIndicatorHandler.UICamera = Camera.main;
        lockRadiusIndicatorHandler.UICamera = Camera.main;
        lockIndicatorHandler.UICamera = Camera.main;
        playerCalloutHandler.UICamera = Camera.main;
        playerArrowCalloutHandler.UICamera = Camera.main;
        statusBarHandler.UICamera = Camera.main;
        // hackDisplay.cam = Camera.main;
        caption.text = "";

        GameManager.OnFocusChanged += BindToNewTarget;
        GameManager.OnCaptionChange += HandleCaptionChange;
        canvas.enabled = true;
        gunDisplayCanvas.enabled = true;
        HideVRStats();
        HideTerminal();
        HideBurglar();
        HideCutsceneDialogue();
        HideCutsceneText();
        // HideMissionSelector();
        saveIndicatorController.HideIndicator();
        objectivesCompleteController.Initialize();
        weaponUIHandler.Initialize();
        meleeWeaponUIHandler.Initialize();
        weaponWheelController.HideWheel();
        weaponWheelController.Initialize();
        targetPracticeUIHandler.canvas.enabled = false;
        statusBarHandler.Initialize();
        HideSoftwareDeployModal();
        keySelectMenu.Hide();
        creditsText.text = "";
        creditsText.color = Color.clear;
        HideAllIndicators();

        if (GameManager.I.playerObject != null)
            BindToNewTarget(GameManager.I.playerObject);
    }
    public void InitializeObjectivesController(GameData data) {
        objectiveCanvasController.Initialize(data);
    }
    public void DisplayObjectiveCompleteMessage(params string[] messages) {
        objectivesCompleteController.DisplayMessage(messages);
    }
    public void UpdateWithPlayerInput(ref PlayerInput input, InputProfile inputProfile) {
        burglarCanvasGroup.interactable = inputProfile.allowBurglarInterface;
        // if (tutorialBurglarInterrupt) return;
        if (burglarCanvas != null && burglarCanvas.enabled && inputProfile.allowBurglarInterface)
            burglarCanvasController?.UpdateWithInput(input);

        aimIndicatorHandler.gameObject.SetActive(!input.revealWeaponWheel);
        weaponWheelController.UpdateWithPlayerInput(ref input);
        lockRadiusIndicatorHandler.gameObject.SetActive(!input.revealWeaponWheel);
    }
    void OnDestroy() {
        GameManager.OnFocusChanged -= BindToNewTarget;
        GameManager.OnCaptionChange -= HandleCaptionChange;
    }

    void BindToNewTarget(GameObject target) {
        // Debug.Log($"ui controller binding to new target {target}");
        weaponUIHandler.Bind(target);
        meleeWeaponUIHandler.Bind(target);

        itemUIHandler.Bind(target);

        aimIndicatorHandler.Bind(target);
        lockRadiusIndicatorHandler.Bind(target);
        lockIndicatorHandler.Bind(target);

        interactiveHighlightHandler.Bind(target);

        visibilityUIHandler.Bind(target);
        suspicionIndicatorHandler.Bind();

        overlayHandler.Bind();
        actionLogHandler.Bind(target);

        healthIndicatorController.Bind(target);
        hitIndicatorController.Bind(target);

        playerArrowCalloutHandler.Bind(target);
    }
    public void LogMessage(string message) {
        if (actionLogHandler != null) {
            actionLogHandler.ShowMessage(message);
        }
    }
    public void ShowTerminal() {
        terminalCanvas.enabled = true;
        terminal.gameObject.SetActive(true);
    }
    public void HideTerminal() {
        terminalCanvas.enabled = false;
        terminal.gameObject.SetActive(false);
    }
    public void ShowBurglar(BurgleTargetData data) {
        burglarMode = true;
        HideUI();
        burglarCanvas.enabled = true;
        burglarCanvasController.Initialize(data);
    }
    public void HideBurglar() {
        burglarMode = false;
        burglarCanvas.enabled = false;
        burglarCanvasController.TearDown();
        // ShowUI();
    }
    // public void ShowMissionSelector(GameData gameData) {
    //     // HideUI();
    //     missionSelectorCanvas.enabled = true;
    //     missionComputerController.Initialize(gameData);
    // }
    // public void HideMissionSelector() {
    //     missionSelectorCanvas.enabled = false;
    //     // ShowUI();
    // }
    void HandleCaptionChange(string newCaption) {
        caption.text = newCaption;
    }
    public void ShowVRStats() {
        if (vRStatHandler == null) return;
        vRStatHandler?.gameObject.SetActive(true);
    }
    public void HideVRStats() {
        if (vRStatHandler == null) return;
        vRStatHandler?.gameObject.SetActive(false);
    }
    public void ShowSoftwareDeployModal() {
        softwareModalController.gameObject.SetActive(true);
    }
    public void HideSoftwareDeployModal() {
        softwareModalController.gameObject.SetActive(false);
    }
    public void HideUI(bool hideKeySelect = true) {
        HideVRStats();
        if (hideKeySelect)
            keySelectMenu.Hide();
        if (canvas != null)
            canvas.enabled = false;
        if (gunDisplayCanvas != null)
            gunDisplayCanvas.enabled = false;
        if (interactiveHighlightCanvas != null)
            interactiveHighlightCanvas.enabled = false;
        if (burglarCanvas != null)
            burglarCanvas.enabled = false;
        if (statusBarCanvas != null)
            statusBarCanvas.enabled = false;
        if (overlayCanvas != null)
            overlayCanvas.enabled = false;
        if (cutsceneTextCanvas != null)
            cutsceneTextCanvas.enabled = false;
        if (cutsceneDialogueCanvas != null)
            cutsceneDialogueCanvas.enabled = false;
        if (indicatorCanvas != null)
            indicatorCanvas.enabled = false;
    }
    public void ShowUI(bool hideCutsceneText = true) {
        if (canvas != null)
            canvas.enabled = true;
        if (gunDisplayCanvas != null)
            gunDisplayCanvas.enabled = true;
        if (interactiveHighlightCanvas != null)
            interactiveHighlightCanvas.enabled = true;
        if (statusBarCanvas != null)
            statusBarCanvas.enabled = true;
        if (burglarMode)
            burglarCanvas.enabled = true;
        if (overlayCanvas != null)
            overlayCanvas.enabled = true;

        if (hideCutsceneText && cutsceneTextCanvas != null)
            cutsceneTextCanvas.enabled = false;
        if (cutsceneDialogueCanvas != null)
            cutsceneDialogueCanvas.enabled = false;
        if (indicatorCanvas != null)
            indicatorCanvas.enabled = true;
        if (GameManager.I.gameData.phase == GamePhase.vrMission) {
            ShowVRStats();
        }
        Debug.Log("show UI");
    }
    public void ShowInteractiveHighlight() {
        interactiveHighlightCanvas.enabled = true;
    }
    public void ShowSaveIndicator() {
        // saveIndicatorController.ShowSaveIndicator();
    }

    public bool OverlayNodeIsSelected() {
        return overlayHandler.selectedNode != null;
    }

    public CameraInput GetOverlayCameraInput() {
        return overlayHandler.selectedNode.GetCameraInput();
    }

    public void ShowCutsceneText(string content) {
        cutsceneTextCanvas.enabled = true;
        tutorialText.enabled = false;
        cutsceneText.enabled = true;
        cutsceneText.text = content;
        StartCoroutine(Toolbox.BlinkEmphasis(cutsceneText, 6, duration: 0.45f));
    }
    public void ShowTutorialText(string content) {
        tutorialText.text = "";
        cutsceneTextCanvas.enabled = true;
        tutorialText.enabled = true;
        cutsceneText.enabled = false;
        StartCoroutine(Toolbox.BlitText(tutorialText, content, interval: 0.02f));

    }
    public void HideCutsceneText() {
        cutsceneTextCanvas.enabled = false;
        // Debug.Log("[cutscene] hide cutscene text");
    }

    public IEnumerator ShowCutsceneDialogue(string name, Sprite portrait, string content, CutsceneDialogueController.Location location = CutsceneDialogueController.Location.bottom) {
        IEnumerator routine = cutsceneDialogueController.Initialize(name, portrait, content, this, location: location);
        Debug.Log("enabling cutscene dialogue canvas");
        cutsceneDialogueCanvas.enabled = true;
        return routine;
    }
    public void HideCutsceneDialogue() {
        tutorialText.text = "";
        cutsceneDialogueCanvas.enabled = false;
    }
    public void ShowKeyMenu(List<DoorLock> doorLocks) {
        keySelectMenu.Initialize(doorLocks);
    }

    public void ShowStatusBar(bool value) {
        statusBarCanvas.enabled = value;
    }
    public void ShowAppearanceInfo(bool value) {
        suspicionIndicatorHandler.gameObject.SetActive(value);
    }
    public void ShowVisibilityInfo(bool value) {
        visibilityUIHandler.gameObject.SetActive(value);
    }
    public void ShowOverlayControls(bool value) {
        overlayCanvas.enabled = value;
    }

    public IEnumerator FadeInCreditsText(string content, float duration) {
        creditsText.text = content;
        creditsText.enabled = true;
        yield return Toolbox.Ease(null, duration, 0f, 1f, PennerDoubleAnimation.Linear, (amount) => {
            Color newColor = Color.white;
            newColor.a = amount;
            creditsText.color = newColor;
        }, unscaledTime: true);
    }
    public IEnumerator FadeOutCreditsText(float duration) {
        yield return Toolbox.Ease(null, duration, 1f, 0f, PennerDoubleAnimation.Linear, (amount) => {
            Color newColor = Color.white;
            newColor.a = amount;
            creditsText.color = newColor;
        }, unscaledTime: true);
        creditsText.enabled = false;
    }

    public void ShowOverlayButtonHighlight(bool value) {
        overlayButtonHighlight.gameObject.SetActive(value);
        if (value) {
            overlayButtonHighlightRoutine = StartCoroutine(Toolbox.Ease(null, 1f, -50f, 0f, PennerDoubleAnimation.BounceEaseOut, (amount) => {
                overlayButtonHighlight.anchoredPosition = new Vector2(0f, amount);
            }, unscaledTime: true, looping: true));
        } else {
            if (overlayButtonHighlightRoutine != null) {
                StopCoroutine(overlayButtonHighlightRoutine);
            }
        }
    }

    public void SetOverlayButtonInteractible(bool value) {
        overlayButtonGroup.interactable = value;
    }

    public void ShowIndicator(RectTransform target, Vector3 offset, IndicatorUIController.Direction direction) {
        indicatorCanvas.enabled = true;
        ShowIndicators(new RectTransform[] { target }, new Vector3[] { offset }, new IndicatorUIController.Direction[] { direction });
    }
    public void ShowIndicators(RectTransform[] targets, Vector3[] offsets, IndicatorUIController.Direction[] directions) {
        indicatorCanvas.enabled = true;
        indicatorUIController.ShowIndicators(targets, offsets, directions);
    }
    public void DrawLine(RectTransform target, Vector3 offset, IndicatorUIController.Origin origin = IndicatorUIController.Origin.top) {
        indicatorCanvas.enabled = true;
        indicatorUIController.DrawLine(target, offset, this, origin: origin);
    }
    public void HideAllIndicators() {
        indicatorUIController.HideAllIndicators();
    }
    public void HideIndicator(RectTransform target) {
        indicatorUIController.HideIndicator(target);
    }
}
