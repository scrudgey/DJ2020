using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelBootstrapper : MonoBehaviour {
    public enum BootStrapType { mission, world, VR, tutorial }
    public BootStrapType type;
    public bool spawnNPCs;
    public LevelTemplate levelTemplate;
    public bool VRMission;
    void Start() {
        StartCoroutine(Toolbox.WaitForSceneLoadingToFinish(Initialize));
    }

    void Initialize() {
        switch (type) {
            case BootStrapType.mission:
                BootStrapMission();
                break;
            case BootStrapType.world:
                BootStrapWorld();
                break;
            case BootStrapType.VR:
                BootStrapVR();
                break;
            case BootStrapType.tutorial:
                BootStrapTutorial();
                break;
        }
    }

    void BootStrapVR() {
        Debug.Log("bootstrapping VR mission...");

        // initialize game state
        GameManager.I.gameData = GameData.TestState();

        // GameManager.I.gameData.playerState.allGuns[0].delta.activeMods.Add(Resources.Load("data/guns/mods/silencer") as GunMod);

        // set up VR mission template
        VRMissionTemplate vrTemplate = VRMissionTemplate.Default();
        vrTemplate.numberConcurrentNPCs = 1;
        VRMissionState state = VRMissionState.Instantiate(vrTemplate);

        // start the game state
        string sceneName = SceneManager.GetActiveScene().name;
        SceneData sceneData = SceneData.loadSceneData(sceneName);
        GameManager.I.StartVRMission(state, sceneData);
    }

    void BootStrapMission() {
        Debug.Log($"bootstrapping mission {levelTemplate.levelName}...");

        // initialize game state
        GameManager.I.gameData = GameData.TestState();
        LevelState level = LevelState.Instantiate(levelTemplate, LevelPlan.Default(GameManager.I.gameData.playerState), GameManager.I.gameData.playerState);

        // select a random extraction point
        string extractionIdn = Toolbox.RandomFromList(GameObject.FindObjectsOfType<ExtractionZone>().Select(zone => zone.data.idn).ToList());
        level.plan.extractionPointIdn = extractionIdn;

        GameManager.I.gameData.levelState = level;

        string sceneName = SceneManager.GetActiveScene().name;
        SceneData sceneData = SceneData.loadSceneData(sceneName);
        // start the game state
        GameManager.I.StartMission(level, sceneData, spawnNpcs: spawnNPCs, doCutscene: false);
    }

    void BootStrapTutorial() {
        Debug.Log($"bootstrapping tutorial {levelTemplate.levelName}...");

        GameData gameData = GameData.DefaultState();
        PlayerState playerState = PlayerState.DefaultState();

        playerState.allItems = new List<ItemTemplate> {
            ItemTemplate.LoadItem("fence_cutters")
        };
        playerState.softwareTemplates = new List<SoftwareScriptableTemplate>{
            SoftwareScriptableTemplate.Load("scan"),
            SoftwareScriptableTemplate.Load("exploit"),
            SoftwareScriptableTemplate.Load("scanData"),
        }.Select(template => template.ToTemplate()).ToList();
        gameData.playerState = playerState;

        // initialize game state
        GameManager.I.gameData = gameData;

        LevelState level = LevelState.Instantiate(levelTemplate, LevelPlan.Default(GameManager.I.gameData.playerState), GameManager.I.gameData.playerState);

        // select a random extraction point
        string extractionIdn = Toolbox.RandomFromList(GameObject.FindObjectsOfType<ExtractionZone>().Select(zone => zone.data.idn).ToList());
        level.plan.extractionPointIdn = extractionIdn;

        GameManager.I.gameData.levelState = level;

        string sceneName = SceneManager.GetActiveScene().name;
        SceneData sceneData = SceneData.loadSceneData(sceneName);

        // start the game state
        GameManager.I.StartMission(level, sceneData, spawnNpcs: spawnNPCs, doCutscene: false);
    }

    void BootStrapWorld() {
        Debug.Log($"bootstrapping world ...");
        GameManager.I.gameData = GameData.TestState();
        GameManager.I.SetMarketData();
        GameManager.I.SetDealData();
        GameManager.I.SetFenceData();
        GameManager.I.SetGunsForSale();
        Scene activeScene = SceneManager.GetActiveScene();
        GameManager.I.StartWorld(activeScene.name);
    }

}
