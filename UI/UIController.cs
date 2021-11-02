using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UI;
public class UIInput {
    public bool toggleConsole;
}

// public partial class UI : Singleton<UI> {
public class UIController : MonoBehaviour {
    public Canvas canvas;
    public TerminalController terminal;
    public WeaponUIHandler weaponUIHandler;
    public ItemUIHandler itemUIHandler;
    public AimIndicatorHandler aimIndicatorHandler;
    public GameObject UIEditorCamera;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    void Start() {
        canvas.worldCamera = Camera.main;
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
        // Debug.Log($"bind: {target}");
        weaponUIHandler.Bind(target);
        itemUIHandler.Bind(target);
        aimIndicatorHandler.Bind(target);

        SetCursor();
    }

    void SetCursor() {
        Texture2D mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 0") as Texture2D;

        Vector2 hotSpot = new Vector2(8, 8);
        CursorMode cursorMode = CursorMode.Auto;
        Cursor.SetCursor(mouseCursor, hotSpot, cursorMode);
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
