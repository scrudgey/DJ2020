using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public enum GameState { none, levelPlay, inMenu }
public enum MenuType { none, console }
public class GameManager : Singleton<GameManager> {
    public static Action<GameObject> OnTargetChanged;

    public static Action OnMenuClosed;
    public static Action<MenuType> OnMenuChange;

    public MenuType activeMenuType;
    public GameData gameData;
    public GameObject playerObject;
    public InputActionReference showConsole;
    private bool toggleConsoleThisFrame;

    public void Start() {
        // TODO: set level in gamedata
        gameData = GameData.TestInitialData();

        TransitionToState(GameState.levelPlay);

        // System
        showConsole.action.performed += ctx => {
            toggleConsoleThisFrame = ctx.ReadValueAsButton();
        };
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
                InitializeLevel();
                break;
            case GameState.inMenu:
                Time.timeScale = 1f;
                break;
            default:
                break;
        }
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
    public void SetFocus(GameObject focus) {
        this.playerObject = focus;
        OnTargetChanged?.Invoke(focus);
    }
    private void InitializeLevel() { // TODO: level enum input
        SetFocus(GameObject.Find("playerCharacter"));

        LoadPlayerState(gameData.playerState);
    }
    public void LoadPlayerState(PlayerData data) {
        foreach (ISaveable saveable in playerObject.GetComponentsInChildren<ISaveable>()) {
            // Debug.Log("triggering load on " + saveable);
            saveable.LoadState(data);
        }
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
