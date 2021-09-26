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
    public GameObject UICamera;
    void Awake() {
        DestroyImmediate(UICamera);
    }
    void Start() {
        canvas.worldCamera = Camera.main;
        GameManager.OnTargetChanged += BindToNewTarget;
        GameManager.OnMenuChange += HandleMenuChange;
        GameManager.OnMenuClosed += HandleMenuClosed;
        BindToNewTarget(GameManager.I.playerObject);
    }
    void OnDestroy() {
        GameManager.OnTargetChanged -= BindToNewTarget;
        GameManager.OnMenuChange -= HandleMenuChange;
        GameManager.OnMenuClosed -= HandleMenuClosed;
    }

    void BindToNewTarget(GameObject target) {
        // Debug.Log($"bind: {target}");
        weaponUIHandler.Bind(target);
        itemUIHandler.Bind(target);
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
