using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { none, levelPlay }
public class GameManager : Singleton<GameManager> {
    public GameData gameData;
    public GameObject playerObject;
    public void Start() {
        // TODO: set level in gamedata
        gameData = GameData.TestInitialData();

        TransitionToState(GameState.levelPlay);
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
            default:
                break;
        }
    }
    public void OnStateExit(GameState state, GameState toState) {
        switch (state) {
            case GameState.none:
                InitializeLevel();
                break;
            default:
                break;
        }
    }
    public void SetFocus(GameObject focus) {
        this.playerObject = focus;
        UI.I.SetFocus(focus);
    }
    private void InitializeLevel() { // TODO: level enum input
        SetFocus(GameObject.Find("playerCharacter"));

        LoadPlayerState(gameData.playerState);
    }
    public void LoadPlayerState(PlayerData data) {
        foreach (ISaveable saveable in playerObject.GetComponentsInChildren<ISaveable>()) {
            Debug.Log("loading " + saveable.ToString());
            saveable.LoadState(data);
        }
    }
}
