using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionPlanController : MonoBehaviour {
    public enum PaneType { loadout, map, tactics, overview }

    public bool debugStart;
    public AudioClip[] buttonSound;
    public AudioClip[] startSound;
    public AudioSource audioSource;

    public LevelTemplate template;
    public LevelPlan plan;
    public GameData gameData;
    public TextMeshProUGUI titleText;

    [Header("Panes")]
    public GameObject overviewPane;
    public GameObject loadOutPane;
    public GameObject mapPane;
    public GameObject tacticPane;
    [Header("Controllers")]
    public MissionPlanLoadoutController loadoutController;
    public MissionPlanMapController mapController;
    public MissionPlanTacticsController tacticsController;
    public MissionPlanOverviewController overviewController;
    [Header("Highlights")]
    public Image overviewHighlight;
    public Image loadoutHighlight;
    public Image mapHighlight;
    public Image tacticsHighlight;

    void Start() {
        Toolbox.RandomizeOneShot(audioSource, startSound, randomPitchWidth: 0.05f);
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
        tacticsController.Initialize(data, template, plan);
        overviewController.Initialize(data, template, plan);
        SwitchPanes(PaneType.overview);
    }
    public void OverviewButtonCallback() {
        Toolbox.RandomizeOneShot(audioSource, buttonSound, randomPitchWidth: 0.05f);
        SwitchPanes(PaneType.overview);
    }
    public void LoadOutButtonCallback() {
        Toolbox.RandomizeOneShot(audioSource, buttonSound, randomPitchWidth: 0.05f);
        SwitchPanes(PaneType.loadout);
    }
    public void MapButtonCallback() {
        Toolbox.RandomizeOneShot(audioSource, buttonSound, randomPitchWidth: 0.05f);
        SwitchPanes(PaneType.map);
    }
    public void TacticButtonCallback() {
        Toolbox.RandomizeOneShot(audioSource, buttonSound, randomPitchWidth: 0.05f);
        SwitchPanes(PaneType.tactics);
    }
    public void SwitchPanes(PaneType toPane) {
        switch (toPane) {
            case PaneType.loadout:
                titleText.text = "Mission Planning - Loadout";
                loadOutPane.SetActive(true);
                mapPane.SetActive(false);
                tacticPane.SetActive(false);
                overviewPane.SetActive(false);

                overviewHighlight.enabled = false;
                loadoutHighlight.enabled = true;
                mapHighlight.enabled = false;
                tacticsHighlight.enabled = false;
                break;
            case PaneType.map:
                titleText.text = "Mission Planning - Map";
                loadOutPane.SetActive(false);
                mapPane.SetActive(true);
                tacticPane.SetActive(false);
                overviewPane.SetActive(false);

                overviewHighlight.enabled = false;
                loadoutHighlight.enabled = false;
                mapHighlight.enabled = true;
                tacticsHighlight.enabled = false;
                break;
            case PaneType.tactics:
                titleText.text = "Mission Planning - Tactics";
                loadOutPane.SetActive(false);
                mapPane.SetActive(false);
                tacticPane.SetActive(true);
                overviewPane.SetActive(false);

                overviewHighlight.enabled = false;
                loadoutHighlight.enabled = false;
                mapHighlight.enabled = false;
                tacticsHighlight.enabled = true;
                break;
            case PaneType.overview:
                titleText.text = "Mission Planning - Overview";
                loadOutPane.SetActive(false);
                mapPane.SetActive(false);
                tacticPane.SetActive(false);
                overviewPane.SetActive(true);

                overviewHighlight.enabled = true;
                loadoutHighlight.enabled = false;
                mapHighlight.enabled = false;
                tacticsHighlight.enabled = false;
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
        GameManager.I.SaveGameData();
        GameManager.I.ReturnToMissionSelector();
    }
}
