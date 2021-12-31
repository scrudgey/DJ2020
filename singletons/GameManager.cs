using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { none, levelPlay, inMenu }
public enum MenuType { none, console }
public enum CursorType { gun }
public partial class GameManager : Singleton<GameManager> {
    public static Action<GameObject> OnFocusChanged;
    public static Action OnMenuClosed;
    public static Action<MenuType> OnMenuChange;

    public MenuType activeMenuType;
    public GameData gameData;
    public GameObject playerObject;
    public InputActionReference showConsole;
    private bool toggleConsoleThisFrame;
    public bool showDebugRays;
    public void Start() {
        // System
        showConsole.action.performed += ctx => {
            toggleConsoleThisFrame = ctx.ReadValueAsButton();
        };

        // TODO: set level in gamedata
        gameData = GameData.TestInitialData();

        TransitionToState(GameState.levelPlay);

        showDebugRays = true;
    }
    public void TransitionToState(GameState newState) {
        GameState tmpInitialState = gameData.state;
        OnStateExit(tmpInitialState, newState);
        gameData.state = newState;
        OnStateEnter(newState, tmpInitialState);
    }
    private void OnStateEnter(GameState state, GameState fromState) {
        // Debug.Log($"entering state {state} from {fromState}");
        switch (state) {
            case GameState.levelPlay:
                // TODO: data-driven level load
                SetCursor(CursorType.gun);
                break;
            case GameState.inMenu:
                Time.timeScale = 0f;
                break;
            default:
                break;
        }
    }
    public void OnStateExit(GameState state, GameState toState) {
        switch (state) {
            case GameState.none:
                // TODO: move this somewhere else or check that we are going to levelPlay
                InitializeLevel("test");
                break;
            case GameState.inMenu:
                Time.timeScale = 1f;
                break;
            default:
                break;
        }
    }
    void SetCursor(CursorType cursorType) {
        // TODO: support multiple cursor types
        Texture2D mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 0") as Texture2D;

        Vector2 hotSpot = new Vector2(8, 8);
        CursorMode cursorMode = CursorMode.Auto;
        Cursor.SetCursor(mouseCursor, hotSpot, cursorMode);
    }
    public void ShowMenu(MenuType menuType) {
        activeMenuType = menuType;
        OnMenuChange(menuType);
        TransitionToState(GameState.inMenu);
    }
    public void CloseMenu() {
        OnMenuClosed();
        TransitionToState(GameState.levelPlay); // this isn't right either?
        activeMenuType = MenuType.none;
    }

    public void Update() {
        if (toggleConsoleThisFrame) {
            if (activeMenuType != MenuType.console) {
                ShowMenu(MenuType.console);
            } else {
                CloseMenu();
            }
        }
        toggleConsoleThisFrame = false;
    }
}
