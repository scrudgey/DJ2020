using System.Collections;
using System.Collections.Generic;
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
    public Canvas missionSelectorCanvas;
    public Canvas interactiveHighlightCanvas;
    public Canvas statusBarCanvas;
    public Canvas overlayCanvas;
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
    public MissionComputerController missionComputerController;
    public SaveIndicatorController saveIndicatorController;
    public WeaponWheelController weaponWheelController;
    public TargetPracticeUIHandler targetPracticeUIHandler;
    public UIStatusBarHandler statusBarHandler;
    public SoftwareModalController softwareModalController;
    public bool mouseOverScrollBox;
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
        HideMissionSelector();
        saveIndicatorController.HideIndicator();
        objectivesCompleteController.Initialize();
        weaponUIHandler.Initialize();
        meleeWeaponUIHandler.Initialize();
        weaponWheelController.HideWheel();
        weaponWheelController.Initialize();
        targetPracticeUIHandler.canvas.enabled = false;
        statusBarHandler.Initialize();
        HideSoftwareDeployModal();

        if (GameManager.I.playerObject != null)
            BindToNewTarget(GameManager.I.playerObject);
    }
    public void InitializeObjectivesController(GameData data) {
        objectiveCanvasController.Initialize(data);
    }
    public void DisplayObjectiveCompleteMessage(params string[] messages) {
        objectivesCompleteController.DisplayMessage(messages);
    }
    public void UpdateWithPlayerInput(ref PlayerInput input) {
        if (burglarCanvas != null && burglarCanvas.enabled)
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
    public void ShowMissionSelector(GameData gameData) {
        // HideUI();
        missionSelectorCanvas.enabled = true;
        missionComputerController.Initialize(gameData);
    }
    public void HideMissionSelector() {
        missionSelectorCanvas.enabled = false;
        // ShowUI();
    }
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
    public void HideUI() {
        HideVRStats();
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
    }
    public void ShowUI() {
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
        if (GameManager.I.gameData.phase == GamePhase.vrMission) {
            ShowVRStats();
        }
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

}
