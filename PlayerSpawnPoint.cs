using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour {
    public GameObject SpawnPlayer(PlayerState state) {
        GameObject playerObject = GameObject.Instantiate(Resources.Load("prefabs/playerCharacter"), transform.position, Quaternion.identity) as GameObject;
        state.ApplyState(playerObject);
        CharacterController controller = playerObject.GetComponentInChildren<CharacterController>();
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();
        controller.OrbitCamera = cam;
        return playerObject;
    }
}