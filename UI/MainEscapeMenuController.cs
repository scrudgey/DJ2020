using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainEscapeMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void ContinueButtonCallback() {
        GameManager.I.CloseMenu();
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
