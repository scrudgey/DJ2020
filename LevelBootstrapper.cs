using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LevelBootstrapper : MonoBehaviour {
    public bool spawnNPCs;
    void Start() {
        Debug.Log("bootstrapping level...");
        GameManager.I.gameData = GameData.TestInitialData();
        VRMissionTemplate vrTemplate = VRMissionTemplate.Default();
        vrTemplate.numberConcurrentNPCs = 0;
        VRMissionState state = VRMissionState.Instantiate(vrTemplate);
        GameManager.I.StartVRMission(state);

        if (spawnNPCs) {
            foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToList()) {
                spawnPoint.SpawnTemplated();
            }
        }
    }

}
