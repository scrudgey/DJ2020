using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class EscapeMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    public InputActionReference escapeAction;


    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
        escapeAction.action.performed += HandleEscapeAction;
    }
    public void Start() {
        foreach (Transform child in objectivesContainer) {
            Destroy(child.gameObject);
        }

        GameData gameData = GameManager.I.gameData;
        foreach (Objective objective in gameData.levelState.template.objectives) {
            GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
            obj.transform.SetParent(objectivesContainer, false);
            PauseScreenObjectiveIndicator controller = obj.GetComponent<PauseScreenObjectiveIndicator>();
            controller.Configure(objective, gameData);
        }
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
