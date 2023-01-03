using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class MissionPlanController : MonoBehaviour {
    public enum PaneType { loadout, map, tactics }

    public bool debugStart;
    public AudioClip[] buttonSound;
    public AudioClip[] startSound;
    public AudioSource audioSource;

    public LevelTemplate template;
    public LevelPlan plan;
    public GameData gameData;

    public GameObject loadOutPane;
    public GameObject mapPane;
    public GameObject tacticPane;
    public TextMeshProUGUI titleText;
    public MissionPlanLoadoutController loadoutController;
    public MissionPlanMapController mapController;

    void Start() {
        Toolbox.RandomizeOneShot(audioSource, startSound);
        if (debugStart) {
            GameManager.I.gameData = GameData.TestInitialData();
            LevelTemplate template = LevelTemplate.LoadAsInstance("Jack That Data");
            Initialize(GameManager.I.gameData, template);
        }
    }

    public void Initialize(GameData data, LevelTemplate template) {
        this.gameData = data;
        this.template = template;
        plan = gameData.GetLevelPlan(template);
        loadoutController.Initialize(data, template, plan);
        mapController.Initialize(data, template, plan);
        SwitchPanes(PaneType.loadout);
    }
    public void LoadOutButtonCallback() {
        Toolbox.RandomizeOneShot(audioSource, buttonSound);
        SwitchPanes(PaneType.loadout);
    }
    public void MapButtonCallback() {
        Toolbox.RandomizeOneShot(audioSource, buttonSound);
        SwitchPanes(PaneType.map);
    }
    public void TacticButtonCallback() {
        Toolbox.RandomizeOneShot(audioSource, buttonSound);
        SwitchPanes(PaneType.tactics);
    }
    public void SwitchPanes(PaneType toPane) {
        switch (toPane) {
            case PaneType.loadout:
                titleText.text = "Mission Planning - Loadout";
                loadOutPane.SetActive(true);
                mapPane.SetActive(false);
                tacticPane.SetActive(false);
                break;
            case PaneType.map:
                titleText.text = "Mission Planning - Map";
                loadOutPane.SetActive(false);
                mapPane.SetActive(true);
                tacticPane.SetActive(false);
                break;
            case PaneType.tactics:
                titleText.text = "Mission Planning - Tactics";
                loadOutPane.SetActive(false);
                mapPane.SetActive(false);
                tacticPane.SetActive(true);
                break;
        }
    }

    public void StartButtonCallback() {
        Debug.Log("start mission");
        GameManager.I.LoadMission(template, plan);
    }

    public void CancelButtonCallback() {
        Debug.Log("cancel plan");
        // return to apartment
        GameManager.I.ReturnToMissionSelector();
    }
}
