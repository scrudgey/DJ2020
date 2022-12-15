using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class EscapeMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    public InputActionReference escapeAction;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
        escapeAction.action.performed += HandleEscapeAction;
    }
    void OnDestroy() {
        escapeAction.action.performed -= HandleEscapeAction;
    }
    public void ContinueButtonCallback() {
        GameManager.I.CloseMenu();
    }
    public void AbortButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.ReturnToTitleScreen();
    }
    public void HandleEscapeAction(InputAction.CallbackContext ctx) {
        ContinueButtonCallback();
    }
}
