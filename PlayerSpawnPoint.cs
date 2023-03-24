using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MapMarker {
    public GameObject SpawnPlayer(PlayerState state, LevelPlan plan) {
        GameObject playerObject = GameObject.Instantiate(Resources.Load("prefabs/playerCharacter"), transform.position, Quaternion.identity) as GameObject;
        state.ApplyState(playerObject);
        plan.ApplyState(playerObject);
        CharacterController controller = playerObject.GetComponentInChildren<CharacterController>();
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();
        LegsAnimation legsAnimation = playerObject.GetComponentInChildren<LegsAnimation>();
        legsAnimation.characterCamera = cam;
        controller.OrbitCamera = cam;
        return playerObject;
    }
}
