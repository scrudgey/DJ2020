using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCSpawnPoint : MonoBehaviour {
    public bool isStrikeTeamSpawn;
    public NPCState npcState;
    public GameObject SpawnNPC() {
        PrefabPool pool = PoolManager.I.RegisterPool("prefabs/NPC", poolSize: 6);
        GameObject npc = pool.GetObject(transform.position);

        PatrolRoute route = GameObject.FindObjectOfType<PatrolRoute>();
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();

        controller.OrbitCamera = cam;
        ai.patrolRoute = route;
        motor.SetPosition(transform.position, bypassInterpolation: true);
        ApplyPlayerState(npc);

        return npc;
    }
    void ApplyPlayerState(GameObject npcObject) {

        npcState.ApplyState(npcObject);
    }
}
