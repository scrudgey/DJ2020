using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { none, levelPlay, inMenu }
public enum MenuType { none, console, dialogue }
public enum OverlayType { none, power, cyber, alarm }
public enum CursorType { none, gun, pointer }
public enum InputMode { none, gun, cyber, aim, wallpressAim }
public struct PointerData {
    public Texture2D mouseCursor;
    public Vector2 hotSpot;
    public CursorMode cursorMode;
}
public partial class GameManager : Singleton<GameManager> {
    public GameData gameData;
    public AudioSource audioSource;
    public GameObject playerObject;
    public PlayerOutlineHandler playerOutlineHandler;
    public LightLevelProbe playerLightLevelProbe;
    // UI input
    public InputActionReference showConsole;

    // UI callbacks
    public static Action OnMenuClosed;
    public static Action<GameObject> OnFocusChanged;
    public static Action<MenuType> OnMenuChange;
    public static Action<PowerGraph> OnPowerGraphChange;
    public static Action<CyberGraph> OnCyberGraphChange;
    public static Action<AlarmGraph> OnAlarmGraphChange;
    public static Action<OverlayType> OnOverlayChange;
    public static Action<InputMode, InputMode> OnInputModeChange; // TODO: legit? should be camera state change?
    public static Action<CursorType> OnCursorTypeChange;
    public static Action<String> OnCaptionChange;
    public static Action<PlayerState> OnEyeVisibilityChange;
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
            if (_cursorType != value)
                SetCursor(value);
            _cursorType = value;
        }
    }
    private InputMode _inputMode;
    public InputMode inputMode {
        get { return _inputMode; }
    }
    int numberFrames;
    public bool showDebugRays;
    public void Start() {
        cursorType = CursorType.pointer;
        showDebugRays = true;
        showConsole.action.performed += HandleShowConsleAction;
        CyberNodeIndicator.staticOnMouseOver += HandleCyberNodeMouseOver;
        CyberNodeIndicator.staticOnMouseExit += HandleCyberNodeMouseExit;
    }
    void HandleShowConsleAction(InputAction.CallbackContext ctx) {
        toggleConsoleThisFrame = ctx.ReadValueAsButton();
    }
    void LateUpdate() {
        if (numberFrames == 1 && SceneManager.GetActiveScene().name != "title") {
            // TODO: set level in gamedata
            gameData = GameData.TestInitialData();
            // TODO: a better start of level method?
            TransitionToState(GameState.levelPlay);
        }
        numberFrames += 1;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        showConsole.action.performed -= HandleShowConsleAction;
        CyberNodeIndicator.staticOnMouseOver -= HandleCyberNodeMouseOver;
        CyberNodeIndicator.staticOnMouseExit -= HandleCyberNodeMouseExit;
    }
    public void TransitionToState(GameState newState) {
        GameState tmpInitialState = gameData.state;
        OnStateExit(tmpInitialState, newState);
        gameData.state = newState;
        OnStateEnter(newState, tmpInitialState);
    }
    public void TransitionToInputMode(InputMode newInputMode) {
        if (newInputMode == inputMode)
            return;
        // Debug.Log($"transition to {newInputMode}");
        OnInputModeChange?.Invoke(inputMode, newInputMode);
        _inputMode = newInputMode;
    }
    private void OnStateEnter(GameState state, GameState fromState) {
        // Debug.Log($"entering state {state} from {fromState}");
        switch (state) {
            case GameState.levelPlay:
                Time.timeScale = 1f;
                cursorType = CursorType.gun;
                TransitionToInputMode(InputMode.gun);
                if (!SceneManager.GetSceneByName("UI").isLoaded) {
                    SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);
                }
                break;
            case GameState.inMenu:
                TransitionToInputMode(InputMode.none);
                Time.timeScale = 0f;
                break;
            default:
                break;
        }
    }
    public void OnStateExit(GameState state, GameState toState) {
        switch (state) {
            case GameState.none:
                break;
            case GameState.inMenu:
                Time.timeScale = 1f;
                break;
            default:
                break;
        }
    }

    void SetCursor(CursorType cursorType) {
        PointerData data = cursorType switch {
            CursorType.pointer => new PointerData() {
                // mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 5") as Texture2D,
                // hotSpot = new Vector2(8, 8),
                mouseCursor = Resources.Load("sprites/UI/elements/Cursor") as Texture2D,
                hotSpot = new Vector2(0, 0),
                cursorMode = CursorMode.Auto
            },
            CursorType.gun => new PointerData {
                mouseCursor = Resources.Load("sprites/UI/elements/Aimpoint/Cursor/Aimpoint16 0") as Texture2D,
                hotSpot = new Vector2(8, 8),
                cursorMode = CursorMode.Auto
            },
            _ => new PointerData()
        };
        Cursor.SetCursor(data.mouseCursor, data.hotSpot, data.cursorMode);
    }
    public void ShowMenu(MenuType menuType) {
        activeMenuType = menuType;
        if (menuType == MenuType.dialogue) {
            Time.timeScale = 0f;
        } else {

        }
        OnMenuChange?.Invoke(menuType);
        // TransitionToState(GameState.inMenu);
    }
    public void CloseMenu() {
        OnMenuClosed?.Invoke();
        Time.timeScale = 1f;
        // TransitionToState(GameState.levelPlay); // this isn't right either?
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
        activeOverlayType = newType;
        OnOverlayChange?.Invoke(newType);
    }

    public void Update() {
        // TODO: disable update if we are loading
        if (toggleConsoleThisFrame) {
            if (activeMenuType != MenuType.console) {
                ShowMenu(MenuType.console);
            } else {
                CloseMenu();
            }
        }
        toggleConsoleThisFrame = false;
        if (gameData.state == GameState.levelPlay) {
            UpdateSuspicion();
            UpdateAlarm();
            UpdateReportTickets();
            UpdateGraphs();
        }
    }

    public void HandleCyberNodeMouseOver(NodeIndicator<CyberNode, CyberGraph> indicator) {
        cursorType = CursorType.pointer;
        TransitionToInputMode(InputMode.cyber); // TODO: ?
    }
    public void HandleCyberNodeMouseExit(NodeIndicator<CyberNode, CyberGraph> indicator) {
        cursorType = CursorType.gun;
        TransitionToInputMode(InputMode.gun);
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
        bool isVisible = lightLevel switch {
            0 => distance < 2f,
            1 => distance < 3f,
            2 => distance < 6f,
            3 => distance < 8f,
            4 => distance < 15f,
            _ => distance < 50f
        };

        return isVisible;
    }
}
