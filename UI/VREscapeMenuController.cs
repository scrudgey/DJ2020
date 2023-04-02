using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VREscapeMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    public TextMeshProUGUI descriptionText;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void ContinueButtonCallback() {
        GameManager.I.CloseMenu();
    }
    public void AbortButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.ReturnToTitleScreen();
    }
    // public void HandleEscapeAction(InputAction.CallbackContext ctx) {
    //     ContinueButtonCallback();
    // }
}
