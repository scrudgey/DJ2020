using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
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

        // set up VR mission template
        VRMissionTemplate vrTemplate = VRMissionTemplate.Default();
        vrTemplate.numberConcurrentNPCs = 0;
        VRMissionState state = VRMissionState.Instantiate(vrTemplate);

        // start the game state
        GameManager.I.StartVRMission(state);
    }

    void BootStrapMission() {
        Debug.Log($"bootstrapping mission {levelTemplate.levelName}...");

        LevelState level = LevelState.Instantiate(levelTemplate, LevelPlan.Default(new List<Items.BaseItem>()));

        level.plan.items.Add(ItemInstance.LoadItem("deck"));
        level.plan.items.Add(ItemInstance.LoadItem("tools"));

        // initialize game state
        GameManager.I.gameData = GameData.TestInitialData() with {
            levelState = level
        };

        // start the game state
        GameManager.I.StartMission(level, spawnNpcs: spawnNPCs);
    }

    void BootStrapWorld() {
        Debug.Log($"bootstrapping world ...");
        GameManager.I.gameData = GameData.TestInitialData();
        GameManager.I.SetMarketData();
        GameManager.I.StartWorld("Apartment");
    }

}
