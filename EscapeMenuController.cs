using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class EscapeMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
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
    public void ContinueButtonCallback() {
        GameManager.I.CloseMenu();
    }
    public void AbortButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.HandleObjectiveFailed();
    }
    public void HandleEscapeAction(InputAction.CallbackContext ctx) {
        ContinueButtonCallback();
    }

    public void SkillMenuCallback() {
        GameManager.I.ShowPerkMenu();
    }
}
