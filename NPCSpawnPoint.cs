using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCSpawnPoint : MonoBehaviour {
    public bool isStrikeTeamSpawn;
    public GameObject spawnEffect;
    public NPCTemplate myTemplate;
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

        PatrolRoute route = Toolbox.RandomFromList(GameObject.FindObjectsOfType<PatrolRoute>());
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();

        controller.OrbitCamera = cam;
        ai.patrolRoute = route;

        // should be part of apply state?
        ai.etiquettes = template.etiquettes;
        ai.portrait = template.portrait;
        motor.SetPosition(transform.position, bypassInterpolation: true);
        ApplyNPCState(template, npc);

        ai.Initialize();
        return npc;
    }
    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.ApplyState(npcObject);
    }

    public void SpawnTemplated() {
        SpawnNPC(myTemplate, useSpawnEffect: false);
    }
}
