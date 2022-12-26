using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LevelBootstrapper : MonoBehaviour {
    public bool spawnNPCs;
    public LevelTemplate levelTemplate;
    public bool VRMission;
    void Start() {

        if (VRMission) {
            BootStrapVR();
        } else {
            BootStrapMission();
        }

        if (spawnNPCs) {
            foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToList()) {
                spawnPoint.SpawnTemplated();
            }
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

        LevelState level = LevelState.Instantiate(levelTemplate);

        // initialize game state
        GameManager.I.gameData = GameData.TestInitialData() with {
            levelState = level
        };


        // start the game state
        GameManager.I.StartMission(level);
    }

}
