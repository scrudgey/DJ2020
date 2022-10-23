using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class UIController : MonoBehaviour {
    public Canvas canvas;
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
    public DialogueController dialogueController;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    void Start() {
        // cameras
        canvas.worldCamera = Camera.main;
        interactiveHighlightHandler.cam = Camera.main;
        overlayHandler.cam = Camera.main;
        aimIndicatorHandler.UICamera = Camera.main;
        lockRadiusIndicatorHandler.UICamera = Camera.main;
        lockIndicatorHandler.UICamera = Camera.main;
        playerCalloutHandler.UICamera = Camera.main;
        hackDisplay.cam = Camera.main;

        GameManager.OnFocusChanged += BindToNewTarget;
        GameManager.OnMenuChange += HandleMenuChange;
        GameManager.OnMenuClosed += HandleMenuClosed;
        GameManager.OnCaptionChange += HandleCaptionChange;
        caption.text = "";
        if (GameManager.I.playerObject != null)
            BindToNewTarget(GameManager.I.playerObject);
        HideVRStats();
        dialogueController.gameObject.SetActive(false);
    }
    void OnDestroy() {
        GameManager.OnFocusChanged -= BindToNewTarget;
        GameManager.OnMenuChange -= HandleMenuChange;
        GameManager.OnMenuClosed -= HandleMenuClosed;
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

    void HandleMenuChange(MenuType type) {
        dialogueController.gameObject.SetActive(false);
        terminal.gameObject.SetActive(false);
        if (type == MenuType.console) {
            terminal.gameObject.SetActive(true);
        } else if (type == MenuType.dialogue) {
            dialogueController.gameObject.SetActive(true);
            dialogueController.Initialize();
        }
    }
    void HandleMenuClosed() {
        terminal.gameObject.SetActive(false);
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
        Debug.Log("hide UI");
        HideVRStats();
        weaponUIHandler.gameObject.SetActive(false);
        itemUIHandler.gameObject.SetActive(false);
        aimIndicatorHandler.gameObject.SetActive(false);
        lockRadiusIndicatorHandler.gameObject.SetActive(false);
        lockIndicatorHandler.gameObject.SetActive(false);
        interactiveHighlightHandler.gameObject.SetActive(false);
        visibilityUIHandler.gameObject.SetActive(false);
        suspicionIndicatorHandler.gameObject.SetActive(false);
        overlayHandler.gameObject.SetActive(false);
        actionLogHandler.gameObject.SetActive(false);
        healthIndicatorController.gameObject.SetActive(false);
        hitIndicatorController.gameObject.SetActive(false);
    }
    public void ShowUI() {
        Debug.Log("show UI");
        ShowVRStats();
        weaponUIHandler.gameObject.SetActive(true);
        itemUIHandler.gameObject.SetActive(true);
        aimIndicatorHandler.gameObject.SetActive(true);
        lockRadiusIndicatorHandler.gameObject.SetActive(true);
        lockIndicatorHandler.gameObject.SetActive(true);
        interactiveHighlightHandler.gameObject.SetActive(true);
        visibilityUIHandler.gameObject.SetActive(true);
        suspicionIndicatorHandler.gameObject.SetActive(true);
        overlayHandler.gameObject.SetActive(true);
        actionLogHandler.gameObject.SetActive(true);
        healthIndicatorController.gameObject.SetActive(true);
        hitIndicatorController.gameObject.SetActive(true);
    }
}
