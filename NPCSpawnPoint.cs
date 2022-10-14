using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCSpawnPoint : MonoBehaviour {
    public bool isStrikeTeamSpawn;
    public GameObject spawnEffect;
    PrefabPool effectPool;
    PrefabPool NPCPool;
    void Start() {
        InitializePools();
    }
    void InitializePools() {
        NPCPool = PoolManager.I?.RegisterPool("prefabs/NPC", poolSize: 10);
        effectPool = PoolManager.I?.RegisterPool(spawnEffect, poolSize: 5);
    }
    public GameObject SpawnNPC(NPCTemplate template) {
        if (effectPool == null) {
            InitializePools();
        }
        effectPool.GetObject(transform.position);
        GameObject npc = NPCPool.GetObject(transform.position);

        PatrolRoute route = Toolbox.RandomFromList(GameObject.FindObjectsOfType<PatrolRoute>());
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();

        controller.OrbitCamera = cam;
        ai.patrolRoute = route;
        motor.SetPosition(transform.position, bypassInterpolation: true);
        ApplyNPCState(template, npc);

        return npc;
    }
    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.ApplyState(npcObject);
    }
}
