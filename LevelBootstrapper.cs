using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBootstrapper : MonoBehaviour {
    void Start() {
        Debug.Log("bootstrapping level...");
        GameManager.I.gameData = GameData.TestInitialData();
        VRMissionTemplate vrTemplate = VRMissionTemplate.Default();
        vrTemplate.numberConcurrentNPCs = 0;
        VRMissionState state = VRMissionState.Instantiate(vrTemplate);
        GameManager.I.StartVRMission(state);
    }

}
