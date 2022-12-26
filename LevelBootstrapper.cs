using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LevelBootstrapper : MonoBehaviour {
    public bool spawnNPCs;
    public LevelTemplate levelTemplate;
    void Start() {
        Debug.Log("bootstrapping level...");

        // initialize game state
        GameManager.I.gameData = GameData.TestInitialData();

        // set up VR mission template
        VRMissionTemplate vrTemplate = VRMissionTemplate.Default();
        vrTemplate.numberConcurrentNPCs = 0;
        VRMissionState state = VRMissionState.Instantiate(vrTemplate);

        // start the game state
        GameManager.I.StartVRMission(state);

        if (spawnNPCs) {
            foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToList()) {
                spawnPoint.SpawnTemplated();
            }
        }
    }

}
