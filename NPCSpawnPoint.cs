using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCSpawnPoint : MonoBehaviour {
    public bool isStrikeTeamSpawn;
    public GameObject spawnEffect;
    public NPCTemplate myTemplate;
    public PatrolRoute[] patrolRoutes;
    PrefabPool effectPool;
    PrefabPool NPCPool;
    void Start() {
        InitializePools();
    }
    void InitializePools() {
        NPCPool = PoolManager.I?.RegisterPool("prefabs/NPC", poolSize: 10);
        effectPool = PoolManager.I?.RegisterPool(spawnEffect, poolSize: 5);
    }
    public GameObject SpawnNPC(NPCTemplate template, bool useSpawnEffect = true) {
        if (effectPool == null) {
            InitializePools();
        }
        effectPool.GetObject(transform.position);
        GameObject npc = NPCPool.GetObject(transform.position);

        PatrolRoute route = Toolbox.RandomFromList(patrolRoutes != null && patrolRoutes.Length > 0 ? patrolRoutes : GameObject.FindObjectsOfType<PatrolRoute>());
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();
        LegsAnimation legsAnimation = npc.GetComponentInChildren<LegsAnimation>();
        legsAnimation.characterCamera = cam;
        controller.OrbitCamera = cam;
        ai.patrolRoute = route;
        ai.prefabPool = NPCPool;

        motor.SetPosition(transform.position, bypassInterpolation: true);
        ApplyNPCState(template, npc);

        ai.Initialize();
        return npc;
    }
    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);

        // TODO: driven by level data
        state.activeGun = 1;
        state.ApplyState(npcObject);
    }

    public GameObject SpawnTemplated() {
        return SpawnNPC(myTemplate, useSpawnEffect: false);
    }
}
