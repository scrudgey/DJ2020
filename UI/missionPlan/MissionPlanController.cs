using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionPlanController : MonoBehaviour {
    public enum PaneType { loadout, map, tactics, overview, cyberdeck }
    public Canvas myCanvas;

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
    public GameObject cyberdeckPane;
    [Header("Controllers")]
    public MissionPlanLoadoutController loadoutController;
    // public MissionPlanMapController mapController;
    public MapDisplay3DView mapDisplay;
    public MissionPlanTacticsController tacticsController;
    public MissionPlanOverviewController overviewController;
    public MissionPlanCyberController cyberController;


    [Header("Highlights")]
    public Image overviewHighlight;
    public Image loadoutHighlight;
    public Image mapHighlight;
    public Image tacticsHighlight;
    public Image cyberdeckHighlight;

    void Awake() {
        myCanvas.enabled = false;
    }

    void Start() {
        if (debugStart) {
            GameManager.I.gameData = GameData.TestInitialData();
            GunTemplate gun1 = GunTemplate.Load("p1");
            GunTemplate gun2 = GunTemplate.Load("s1");
            GunTemplate gun3 = GunTemplate.Load("r1");
            GunTemplate gun4 = GunTemplate.Load("p2");
            GunState gunState1 = GunState.Instantiate(gun1);
            GunState gunState2 = GunState.Instantiate(gun2);
            GunState gunState3 = GunState.Instantiate(gun3);
            GunState gunState4 = GunState.Instantiate(gun4);

            GameManager.I.gameData.playerState.allGuns.Add(new WeaponState(gunState1));
            GameManager.I.gameData.playerState.allGuns.Add(new WeaponState(gunState2));
            GameManager.I.gameData.playerState.allGuns.Add(new WeaponState(gunState3));
            GameManager.I.gameData.playerState.allGuns.Add(new WeaponState(gunState4));

            ItemTemplate item1 = ItemTemplate.LoadItem("C4");
            ItemTemplate item2 = ItemTemplate.LoadItem("rocket");
            ItemTemplate item3 = ItemTemplate.LoadItem("grenade");

            GameManager.I.gameData.playerState.allItems.Add(item1);
            GameManager.I.gameData.playerState.allItems.Add(item2);
            GameManager.I.gameData.playerState.allItems.Add(item3);

            // LevelTemplate template = LevelTemplate.LoadAsInstance("Jack That Data");
            LevelTemplate template = LevelTemplate.LoadResource("Jack That Data");
            Initialize(GameManager.I.gameData, template);
        }
    }

    public void Initialize(GameData data, LevelTemplate template) {
        this.gameData = data;
        this.template = template;
        plan = gameData.GetLevelPlan(template);
        loadoutController.Initialize(data, template, plan);
        // mapController.Initialize(data, template, plan);
        // SceneData sceneData = SceneData.loadSceneData(template.sceneName);

        mapDisplay.Initialize(template, plan, template.initialScene);
        tacticsController.Initialize(data, template, plan);
        overviewController.Initialize(data, template, plan);
        cyberController.Initialize(data, template, plan);
        SwitchPanes(PaneType.overview);
        myCanvas.enabled = true;
    }
    public void OverviewButtonCallback() {
        SwitchPanes(PaneType.overview);
    }
    public void LoadOutButtonCallback() {
        SwitchPanes(PaneType.loadout);
    }
    public void MapButtonCallback() {
        SwitchPanes(PaneType.map);
    }
    public void TacticButtonCallback() {
        SwitchPanes(PaneType.tactics);
    }
    public void CyberButtonCallback() {
        SwitchPanes(PaneType.cyberdeck);
    }
    public void SwitchPanes(PaneType toPane) {
        Toolbox.RandomizeOneShot(audioSource, startSound, randomPitchWidth: 0.05f);
        switch (toPane) {
            case PaneType.loadout:
                titleText.text = "Mission Planning - Loadout";
                loadOutPane.SetActive(true);
                mapPane.SetActive(false);
                tacticPane.SetActive(false);
                overviewPane.SetActive(false);
                cyberdeckPane.SetActive(false);

                overviewHighlight.enabled = false;
                loadoutHighlight.enabled = true;
                mapHighlight.enabled = false;
                tacticsHighlight.enabled = false;
                cyberdeckHighlight.enabled = false;
                break;
            case PaneType.map:
                titleText.text = "Mission Planning - Map";
                loadOutPane.SetActive(false);
                mapPane.SetActive(true);
                tacticPane.SetActive(false);
                overviewPane.SetActive(false);
                cyberdeckPane.SetActive(false);

                overviewHighlight.enabled = false;
                loadoutHighlight.enabled = false;
                mapHighlight.enabled = true;
                tacticsHighlight.enabled = false;
                cyberdeckHighlight.enabled = false;

                break;
            case PaneType.tactics:
                titleText.text = "Mission Planning - Tactics";
                loadOutPane.SetActive(false);
                mapPane.SetActive(false);
                tacticPane.SetActive(true);
                overviewPane.SetActive(false);
                cyberdeckPane.SetActive(false);

                overviewHighlight.enabled = false;
                loadoutHighlight.enabled = false;
                mapHighlight.enabled = false;
                tacticsHighlight.enabled = true;
                cyberdeckHighlight.enabled = false;

                break;
            case PaneType.overview:
                titleText.text = "Mission Planning - Overview";
                loadOutPane.SetActive(false);
                mapPane.SetActive(false);
                tacticPane.SetActive(false);
                overviewPane.SetActive(true);
                cyberdeckPane.SetActive(false);

                overviewHighlight.enabled = true;
                loadoutHighlight.enabled = false;
                mapHighlight.enabled = false;
                tacticsHighlight.enabled = false;
                cyberdeckHighlight.enabled = false;

                break;
            case PaneType.cyberdeck:
                titleText.text = "Mission Planning - Cyberdeck";
                loadOutPane.SetActive(false);
                mapPane.SetActive(false);
                tacticPane.SetActive(false);
                overviewPane.SetActive(false);
                cyberdeckPane.SetActive(true);

                overviewHighlight.enabled = false;
                loadoutHighlight.enabled = false;
                mapHighlight.enabled = false;
                tacticsHighlight.enabled = false;
                cyberdeckHighlight.enabled = true;
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
