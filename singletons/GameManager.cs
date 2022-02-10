using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { none, levelPlay, inMenu }
public enum MenuType { none, console }
public enum OverlayType { none, power, cyber }
public enum CursorType { gun }
public partial class GameManager : Singleton<GameManager> {
    public static Action<GameObject> OnFocusChanged;
    public GameData gameData;
    public GameObject playerObject;


    // UI input
    public InputActionReference showConsole;

    // UI callbacks
    public static Action OnMenuClosed;
    public static Action<MenuType> OnMenuChange;
    public static Action<PowerGraph> OnPowerGraphChange;
    public static Action<CyberGraph> OnCyberGraphChange;
    public static Action<OverlayType> OnOverlayChange;

    // UI state
    private bool toggleConsoleThisFrame;
    private bool nextOverlayThisFrame;
    private bool previousOverlayThisFrame;
    public MenuType activeMenuType;
    public OverlayType activeOverlayType;

    public bool showDebugRays;
    public void Start() {
        // System
        showConsole.action.performed += ctx => {
            toggleConsoleThisFrame = ctx.ReadValueAsButton();
        };

        // TODO: set level in gamedata
        gameData = GameData.TestInitialData();

        // TODO: a better start of level method?
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

    // should there be a UI state holder?
    public void IncrementOverlay(int increment) {
        int current = (int)gameData.overlayIndex;
        int length = Enum.GetNames(typeof(OverlayType)).Length;
        int nextInt = (current + increment);

        if (nextInt < 0) {
            nextInt = length - 1;
        } else if (nextInt >= length) {
            nextInt = 0;
        }

        OverlayType newType = (OverlayType)nextInt;
        SetOverlay(newType);
    }
    public void SetOverlay(OverlayType newType) {
        gameData.overlayIndex = (int)newType;
        OnOverlayChange?.Invoke(newType);
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
