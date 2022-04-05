using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class UIInput {
    public bool toggleConsole;
}

public class MouseInputData {
    public Vector3 mousePosition;
}
public class UIController : MonoBehaviour {
    public Canvas canvas;
    public TerminalController terminal;
    public WeaponUIHandler weaponUIHandler;
    public ItemUIHandler itemUIHandler;
    public AimIndicatorHandler aimIndicatorHandler;
    public InteractiveHighlightHandler interactiveHighlightHandler;
    public ActionLogHandler actionLogHandler;
    public OverlayHandler overlayHandler;
    public GameObject UIEditorCamera;
    public HackDisplay hackDisplay;
    public VisibilityUIHandler visibilityUIHandler;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    void Start() {
        // cameras
        canvas.worldCamera = Camera.main;
        interactiveHighlightHandler.cam = Camera.main;
        overlayHandler.cam = Camera.main;
        aimIndicatorHandler.UICamera = Camera.main;
        hackDisplay.cam = Camera.main;

        GameManager.OnFocusChanged += BindToNewTarget;
        GameManager.OnMenuChange += HandleMenuChange;
        GameManager.OnMenuClosed += HandleMenuClosed;
        if (GameManager.I.playerObject != null)
            BindToNewTarget(GameManager.I.playerObject);
    }
    void OnDestroy() {
        GameManager.OnFocusChanged -= BindToNewTarget;
        GameManager.OnMenuChange -= HandleMenuChange;
        GameManager.OnMenuClosed -= HandleMenuClosed;
        GameManager.OnPowerGraphChange -= overlayHandler.RefreshPowerGraph;
        GameManager.OnOverlayChange -= overlayHandler.HandleOverlayChange;
    }

    void BindToNewTarget(GameObject target) {
        // Debug.Log("binding to new target");
        weaponUIHandler.Bind(target);

        itemUIHandler.Bind(target);

        aimIndicatorHandler.Bind(target);
        aimIndicatorHandler.Bind(target);

        interactiveHighlightHandler.Bind(target);

        visibilityUIHandler.Bind(target);

        overlayHandler.Bind();
        actionLogHandler.Bind(target);
    }

    void HandleMenuChange(MenuType type) {
        if (type == MenuType.console) {
            terminal.gameObject.SetActive(true);
        }
    }
    void HandleMenuClosed() {
        terminal.gameObject.SetActive(false);
    }
}
