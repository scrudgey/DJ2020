using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionFailMenuController : MonoBehaviour {
    public Canvas mycanvas;
    public GameObject UIEditorCamera;
    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;
    public GameObject bonusObjectiveHeader;
    public TextMeshProUGUI missionname;
    private GameData gameData;
    void Awake() {
        mycanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
    }
    public void Start() {
        foreach (Transform child in objectivesContainer) {
            if (child.gameObject == bonusObjectiveHeader) continue;
            Destroy(child.gameObject);
        }
    }
    public void Initialize(GameData data) {
        this.gameData = data;
        missionname.text = data.levelState.template.readableMissionName;
        foreach (ObjectiveDelta objective in data.levelState.delta.objectiveDeltas) {
            GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
            obj.transform.SetParent(objectivesContainer, false);
            MissionSelectorObjective controller = obj.GetComponent<MissionSelectorObjective>();
            controller.Initialize(objective);
        }
        if (data.levelState.delta.optionalObjectiveDeltas.Count > 0) {
            bonusObjectiveHeader.SetActive(true);
            bonusObjectiveHeader.transform.SetAsLastSibling();
            foreach (ObjectiveDelta objective in data.levelState.delta.optionalObjectiveDeltas) {
                GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
                obj.transform.SetParent(objectivesContainer, false);
                MissionSelectorObjective controller = obj.GetComponent<MissionSelectorObjective>();
                controller.Initialize(objective, isBonus: true);
            }
        } else {
            bonusObjectiveHeader.SetActive(false);
        }
        mycanvas.enabled = true;
    }
    public void ReplanButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.ShowMissionPlanner(gameData.levelState.template);
    }
    public void RetryButtonCallback() {
        GameManager.I.CloseMenu();
        LevelPlan plan = gameData.GetLevelPlan(gameData.levelState.template);
        GameManager.I.LoadMission(gameData.levelState.template, plan);
    }
    public void ApartmentButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.ReturnToApartment();
    }
}
