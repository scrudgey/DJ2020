using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelBootstrapper : MonoBehaviour {
    public bool spawnNPCs;
    public LevelTemplate levelTemplate;
    public bool VRMission;
    public bool world;
    void Start() {
        StartCoroutine(Toolbox.WaitForSceneLoadingToFinish(Initialize));
    }

    void Initialize() {
        if (world) {
            BootStrapWorld();
        } else if (VRMission) {
            BootStrapVR();
        } else {
            BootStrapMission();
        }
    }

    void BootStrapVR() {
        Debug.Log("bootstrapping VR mission...");

        // initialize game state
        GameManager.I.gameData = GameData.TestInitialData();

        GameManager.I.gameData.playerState.allGuns[0].delta.activeMods.Add(Resources.Load("data/guns/mods/silencer") as GunMod);

        // set up VR mission template
        VRMissionTemplate vrTemplate = VRMissionTemplate.Default();
        vrTemplate.numberConcurrentNPCs = 1;
        VRMissionState state = VRMissionState.Instantiate(vrTemplate);

        // start the game state
        GameManager.I.StartVRMission(state);
    }

    void BootStrapMission() {
        Debug.Log($"bootstrapping mission {levelTemplate.levelName}...");

        List<ItemTemplate> allItems = new List<ItemTemplate> {
            // BaseItem.LoadItem("deck"),
            // ItemTemplate.LoadItem("C4"),
            ItemTemplate.LoadItem("goggles"),
            ItemTemplate.LoadItem("rocket"),
            ItemTemplate.LoadItem("grenade"),
        };

        LevelState level = LevelState.Instantiate(levelTemplate, LevelPlan.Default(allItems));

        // initialize game state
        GameManager.I.gameData = GameData.TestInitialData() with {
            levelState = level
        };

        // start the game state
        GameManager.I.StartMission(level, spawnNpcs: spawnNPCs, doCutscene: false);
    }

    void BootStrapWorld() {
        Debug.Log($"bootstrapping world ...");
        GameManager.I.gameData = GameData.TestInitialData();
        GameManager.I.gameData.playerState.payDatas.Add(Resources.Load("data/paydata/DAT001") as PayData);
        GameManager.I.gameData.playerState.payDatas.Add(Resources.Load("data/paydata/DAT002") as PayData);
        GameManager.I.gameData.playerState.payDatas.Add(Resources.Load("data/paydata/delta_memo") as PayData);
        GameManager.I.gameData.playerState.payDatas.Add(Resources.Load("data/paydata/GLOB003") as PayData);
        GameManager.I.gameData.playerState.payDatas.Add(Resources.Load("data/paydata/hosaka1") as PayData);
        GameManager.I.gameData.playerState.payDatas.Add(Resources.Load("data/paydata/VBS_log") as PayData);
        GameManager.I.gameData.playerState.payDatas.Add(Resources.Load("data/paydata/viral13") as PayData);

        // GameManager.I.gameData.playerState.health = 50f;
        GameManager.I.SetMarketData();
        GameManager.I.SetDealData();
        Scene activeScene = SceneManager.GetActiveScene();
        GameManager.I.StartWorld(activeScene.name);
    }

}
