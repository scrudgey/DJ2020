using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { none, levelPlay, inMenu }
public enum MenuType { none, console }
public enum OverlayType { none, power, cyber }
public enum CursorType { gun, pointer }
public enum InputMode { none, gun, cyber }
public struct CursorData {
    public Texture2D mouseCursor;
    public Vector2 hotSpot;
    public CursorMode cursorMode;
}
public partial class GameManager : Singleton<GameManager> {
    public static Action<GameObject> OnFocusChanged;
    public GameData gameData;
    public GameObject playerObject;
    public LightLevelProbe playerLightLevelProbe;

    // UI input
    public InputActionReference showConsole;

    // UI callbacks
    public static Action OnMenuClosed;
    public static Action<MenuType> OnMenuChange;
    public static Action<PowerGraph> OnPowerGraphChange;
    public static Action<CyberGraph> OnCyberGraphChange;
    public static Action<OverlayType> OnOverlayChange;
    public static Action<InputMode> OnInputModeChange;
    public static Action<CursorType> OnCursorTypeChange;

    // UI state
    private bool toggleConsoleThisFrame;
    private bool nextOverlayThisFrame;
    private bool previousOverlayThisFrame;
    public MenuType activeMenuType;
    public OverlayType activeOverlayType = OverlayType.none;
    private CursorType _cursorType;
    public CursorType cursorType {
        get { return _cursorType; }
        set {
            _cursorType = value;
            SetCursor(value);
        }
    }
    public InputMode inputMode;
    int numberFrames;
    public bool showDebugRays;
    public void Start() {
        // System
        showConsole.action.performed += ctx => {
            toggleConsoleThisFrame = ctx.ReadValueAsButton();
        };

        showDebugRays = true;

        CyberNodeIndicator.staticOnMouseOver += HandleCyberNodeMouseOver;
        CyberNodeIndicator.staticOnMouseExit += HandleCyberNodeMouseExit;
    }
    void LateUpdate() {
        if (numberFrames == 1) {
            Debug.Log("initializing");
            // Debug.Break();
            // TODO: set level in gamedata
            gameData = GameData.TestInitialData();

            // TODO: a better start of level method?
            TransitionToState(GameState.levelPlay);
        }
        numberFrames += 1;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        CyberNodeIndicator.staticOnMouseOver -= HandleCyberNodeMouseOver;
        CyberNodeIndicator.staticOnMouseExit -= HandleCyberNodeMouseExit;
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
                cursorType = CursorType.gun;
                inputMode = InputMode.gun;
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
        CursorData data = cursorType switch {
            CursorType.pointer => new CursorData() {
                // mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 5") as Texture2D,
                // hotSpot = new Vector2(8, 8),
                mouseCursor = Resources.Load("sprites/UI/elements/Cursor") as Texture2D,
                hotSpot = new Vector2(0, 0),
                cursorMode = CursorMode.Auto
            },
            CursorType.gun => new CursorData {
                mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 0") as Texture2D,
                hotSpot = new Vector2(8, 8),
                cursorMode = CursorMode.Auto
            },
            _ => new CursorData()
        };
        Cursor.SetCursor(data.mouseCursor, data.hotSpot, data.cursorMode);
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

    public void HandleCyberNodeMouseOver(NodeIndicator<CyberNode, CyberGraph> indicator) {
        cursorType = CursorType.pointer;
        inputMode = InputMode.cyber;
    }
    public void HandleCyberNodeMouseExit(NodeIndicator<CyberNode, CyberGraph> indicator) {
        cursorType = CursorType.gun;
        inputMode = InputMode.gun;
    }


    public bool IsPlayerVisible(float distance) {

        // B < 10: -
        // 10 < B < 15: +
        // 15 < B < 30: ++
        // 30 < B < 60: +++
        // 60 < B < 80: ++++
        // 80 < B : +++++

        // Toolbox.GetP
        int lightLevel = playerLightLevelProbe.GetDiscreteLightLevel();
        // Debug.Log($"{lightLevel} {distance}");
        switch (lightLevel) {
            case 0:
                return distance < 1f;
            case 1:
                return distance < 2f;
            case 2:
                return distance < 4.5f;
            default:
            case 3:
                return distance < 7f;
            case 4:
                return distance < 13f;
            case 5:
                return distance < 50f;
        }
    }
}
