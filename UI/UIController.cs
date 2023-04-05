using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class UIController : MonoBehaviour {
    public Canvas canvas;
    public Canvas terminalCanvas;
    public Canvas burglarCanvas;
    public Canvas missionSelectorCanvas;
    public Canvas interactiveHighlightCanvas;
    public TerminalController terminal;
    public WeaponUIHandler weaponUIHandler;
    public ItemUIHandler itemUIHandler;
    public AimIndicatorHandler aimIndicatorHandler;
    public LockRadiusIndicatorHandler lockRadiusIndicatorHandler;
    public LockIndicatorHandler lockIndicatorHandler;
    public InteractiveHighlightHandler interactiveHighlightHandler;
    public ActionLogHandler actionLogHandler;
    public OverlayHandler overlayHandler;
    public GameObject UIEditorCamera;
    public HackDisplay hackDisplay;
    public VisibilityUIHandler visibilityUIHandler;
    public SuspicionIndicatorHandler suspicionIndicatorHandler;
    public HealthIndicatorController healthIndicatorController;
    public PlayerCalloutHandler playerCalloutHandler;
    public TextMeshProUGUI caption;
    public HitIndicatorController hitIndicatorController;
    public VRStatHandler vRStatHandler;
    public BurglarCanvasController burglarCanvasController;
    public ObjectiveCanvasController objectiveCanvasController;
    public MissionComputerController missionComputerController;
    public SaveIndicatorController saveIndicatorController;
    bool burglarMode;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Start() {
        // cameras
        canvas.worldCamera = Camera.main;
        interactiveHighlightHandler.cam = Camera.main;
        overlayHandler.cam = Camera.main;
        aimIndicatorHandler.UICamera = Camera.main;
        lockRadiusIndicatorHandler.UICamera = Camera.main;
        lockIndicatorHandler.UICamera = Camera.main;
        playerCalloutHandler.UICamera = Camera.main;
        hackDisplay.cam = Camera.main;
        caption.text = "";

        GameManager.OnFocusChanged += BindToNewTarget;
        GameManager.OnCaptionChange += HandleCaptionChange;
        if (GameManager.I.playerObject != null)
            BindToNewTarget(GameManager.I.playerObject);
        canvas.enabled = true;
        HideVRStats();
        HideTerminal();
        HideBurglar();
        HideMissionSelector();
        saveIndicatorController.HideIndicator();
    }
    public void InitializeObjectivesController(GameData data) {
        objectiveCanvasController.Initialize(data);
    }
    public void UpdateWithPlayerInput(PlayerInput input) {
        if (burglarCanvas != null && burglarCanvas.enabled)
            burglarCanvasController?.UpdateWithInput(input);
    }
    void OnDestroy() {
        GameManager.OnFocusChanged -= BindToNewTarget;
        GameManager.OnCaptionChange -= HandleCaptionChange;
    }

    void BindToNewTarget(GameObject target) {
        // Debug.Log("binding to new target");
        weaponUIHandler.Bind(target);

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
        vRStatHandler.gameObject.SetActive(true);
    }
    public void HideVRStats() {
        vRStatHandler.gameObject.SetActive(false);
    }
    public void HideUI() {
        HideVRStats();
        canvas.enabled = false;
        interactiveHighlightCanvas.enabled = false;
        burglarCanvas.enabled = false;
    }
    public void ShowUI() {
        canvas.enabled = true;
        interactiveHighlightCanvas.enabled = true;
        if (burglarMode)
            burglarCanvas.enabled = true;
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

}
