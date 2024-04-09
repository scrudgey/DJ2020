using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainEscapeMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    [Header("sounds")]
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
        GameManager.I.PlayUISound(openSounds);
    }
    public void ContinueButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.PlayUISound(closeSounds);
    }
    public void SaveButtonCallback() {
        GameManager.I.SaveGameData();
        GameManager.I.CloseMenu();
    }
    public void SaveAndQuitCallback() {
        GameManager.I.SaveGameData();
        GameManager.I.ReturnToTitleScreen();
    }
}
