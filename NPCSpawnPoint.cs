using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCSpawnPoint : MonoBehaviour {
    public bool isStrikeTeamSpawn;
    public GameObject spawnEffect;
    public NPCTemplate myTemplate;
    public PatrolRoute[] patrolRoutes;
    public List<LootDropElementWithProbability> lootDrops;

    static PrefabPool effectPool;
    static PrefabPool NPCPool;
    void Start() {
        InitializePools();
    }
    static void InitializePools() {
        NPCPool = PoolManager.I?.RegisterPool("prefabs/NPC", poolSize: 10);
    }
    public GameObject SpawnNPC(NPCTemplate template, bool useSpawnEffect = true) {
        if (effectPool == null) {
            effectPool = PoolManager.I?.RegisterPool(spawnEffect, poolSize: 5);
        }
        effectPool.GetObject(transform.position);
        return SpawnNPC(template, transform.position, patrolRoutes, lootDrops);
    }

    public static GameObject SpawnNPC(NPCTemplate template, Vector3 position, PatrolRoute[] patrolRoutes, List<LootDropElementWithProbability> lootDrops) {
        if (NPCPool == null) {
            InitializePools();
        }
        GameObject npc = NPCPool.GetObject(position);

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

        LootDropper lootDropper = npc.GetComponentInChildren<LootDropper>();
        if (lootDropper != null && lootDrops != null) {
            lootDropper.loot.AddRange(lootDrops);
        }

        motor.SetPosition(position, bypassInterpolation: true);
        ApplyNPCState(template, npc);

        ai.Initialize();
        return npc;
    }

    static void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);

        state.activeGun = 1;
        state.ApplyState(npcObject);
    }

    public GameObject SpawnTemplated() {
        return SpawnNPC(myTemplate, useSpawnEffect: false);
    }
}
