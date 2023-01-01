using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionFailMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;
    private GameData gameData;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Start() {
        foreach (Transform child in objectivesContainer) {
            Destroy(child.gameObject);
        }
    }
    public void Initialize(GameData data) {
        this.gameData = data;
        foreach (Objective objective in data.levelState.template.objectives) {
            GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
            obj.transform.SetParent(objectivesContainer, false);
            PauseScreenObjectiveIndicator controller = obj.GetComponent<PauseScreenObjectiveIndicator>();
            controller.Configure(objective, data);
        }
    }
    public void ReplanButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.ShowMissionPlanner(gameData.levelState.template);
    }
    public void RetryButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.LoadMission(gameData.levelState.template);
    }
}
