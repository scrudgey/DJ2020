using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { none, levelPlay }
public class GameManager : Singleton<GameManager> {
    private GameState _state;
    public GameState state {
        get { return _state; }
        set {
            GameState previous = _state;
            _state = value;
            ChangeGameState(previous);
        }
    }
    public GameObject playerObject;
    public void Start() {
        if (state == GameState.none) {
            state = GameState.levelPlay;
        }
    }
    private void ChangeGameState(GameState previousState) {
        switch (state) {
            case GameState.levelPlay:
                if (previousState == GameState.none)
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
    private void InitializeLevel() {
        SetFocus(GameObject.Find("playerCharacter"));

        // initialize gun instance
        GunHandler gunHandler = playerObject.GetComponentInChildren<GunHandler>();
        gunHandler.primary = new GunInstance(Gun.Load("smg"));
        // gunHandler.primary = new GunInstance(Gun.Load("rifle"));
        gunHandler.secondary = new GunInstance(Gun.Load("pistol"));
        gunHandler.third = new GunInstance(Gun.Load("shotgun"));
        // gunHandler.third = new GunInstance(Gun.Load("smg"));
        gunHandler.SwitchToGun(2);
    }
}
