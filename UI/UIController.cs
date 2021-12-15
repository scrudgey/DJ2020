using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UIInput {
    public bool toggleConsole;
}


public class UIController : MonoBehaviour {
    public Canvas canvas;
    public TerminalController terminal;
    public WeaponUIHandler weaponUIHandler;
    public ItemUIHandler itemUIHandler;
    public AimIndicatorHandler aimIndicatorHandler;
    public InteractionIndicatorHandler interactionIndicatorHandler;
    public InteractiveHighlightHandler interactiveHighlightHandler;
    public ActionLogHandler actionLogHandler;
    public GameObject UIEditorCamera;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    void Start() {
        // cameras
        canvas.worldCamera = Camera.main;
        interactionIndicatorHandler.cam = Camera.main;
        interactiveHighlightHandler.cam = Camera.main;

        aimIndicatorHandler.UICamera = Camera.main;
        GameManager.OnFocusChanged += BindToNewTarget;
        GameManager.OnMenuChange += HandleMenuChange;
        GameManager.OnMenuClosed += HandleMenuClosed;
        BindToNewTarget(GameManager.I.playerObject);
    }
    void OnDestroy() {
        GameManager.OnFocusChanged -= BindToNewTarget;
        GameManager.OnMenuChange -= HandleMenuChange;
        GameManager.OnMenuClosed -= HandleMenuClosed;
    }

    void BindToNewTarget(GameObject target) {
        ((IBinder<GunHandler>)weaponUIHandler).Bind(target);

        ((IBinder<ItemHandler>)itemUIHandler).Bind(target);

        ((IBinder<GunHandler>)aimIndicatorHandler).Bind(target);
        ((IBinder<NeoCharacterController>)aimIndicatorHandler).Bind(target);

        ((IBinder<Interactor>)interactionIndicatorHandler).Bind(target);
        ((IBinder<Interactor>)interactiveHighlightHandler).Bind(target);

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
