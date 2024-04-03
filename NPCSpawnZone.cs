using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.AI;

public class NPCSpawnZone : MonoBehaviour {
    public WalkToStoreState.StoreType storeType;
    public FenceLocation fenceLocation;
    public LoHi number;
    public NPCTemplate[] templates;
    public Collider zone;
    public bool world;
    public bool spawnNPCs = true;
    PrefabPool NPCPool;
    GameObject fencePrefab;
    WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.01f);
    NavMeshHit hit = new NavMeshHit();
    static NavMeshQueryFilter filter;
    void Start() {
        InitializePools();
    }
    void InitializePools() {
        filter = new NavMeshQueryFilter {
            areaMask = LayerUtil.KeySetToNavLayerMask(new HashSet<int>())
        };
        if (world) {
            NPCPool = PoolManager.I?.RegisterPool("prefabs/WorldNPC", poolSize: 100);
        } else {
            NPCPool = PoolManager.I?.RegisterPool("prefabs/CivilianNPC", poolSize: 100);
        }
        fencePrefab = Resources.Load("prefabs/lootShop") as GameObject;
        // effectPool = PoolManager.I?.RegisterPool(spawnEffect, poolSize: 5);
    }
    public List<GameObject> SpawnNPCs() {
        if (spawnNPCs) {
            return SpawnRoutineBlocking();
        } else {
            return new List<GameObject>();
        }
        // StartCoroutine(SpawnRoutine());
    }
    public GameObject SpawnFence(LootBuyerData lootData) {
        GameObject obj = SpawnNPC(lootData.template, (Vector3 pos) => GameObject.Instantiate(fencePrefab, pos, Quaternion.identity));
        StoreOwner storeOwner = obj.GetComponent<StoreOwner>();
        storeOwner.storeType = StoreType.loot;
        storeOwner.lootBuyerData = lootData;

        GameObject watch = new GameObject($"{lootData.buyerName} watchpoint");
        watch.transform.position = samplePosition();
        storeOwner.lookAtPoint = watch.transform;
        return obj;
    }
    List<GameObject> SpawnRoutineBlocking() {
        if (NPCPool == null) {
            InitializePools();
        }
        List<GameObject> npcs = new List<GameObject>();
        for (int i = 0; i < number.GetRandomInsideBound(); i++) {
            NPCTemplate template = Toolbox.RandomFromList(templates);
            npcs.Add(SpawnNPC(template, NPCPool.GetObject));
        }
        return npcs;
    }
    IEnumerator SpawnRoutine() {
        if (NPCPool == null) {
            InitializePools();
        }
        for (int i = 0; i < number.GetRandomInsideBound(); i++) {
            NPCTemplate template = Toolbox.RandomFromList(templates);
            SpawnNPC(template, NPCPool.GetObject);
            yield return wait;
        }
    }


    Vector3 samplePosition() {
        Vector3 point = Toolbox.RandomInsideBounds(zone);
        if (NavMesh.SamplePosition(point, out hit, 1f, filter)) {
            return hit.position;
        } else return Vector3.zero;
    }
    public GameObject SpawnNPC(NPCTemplate template, Func<Vector3, GameObject> spawn) {
        if (NPCPool == null) {
            InitializePools();
        }
        Vector3 destination = samplePosition();

        if (destination != Vector3.zero) {
            GameObject npc = spawn(destination);

            CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

            CharacterController controller = npc.GetComponentInChildren<CharacterController>();
            KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
            LegsAnimation legsAnimation = npc.GetComponentInChildren<LegsAnimation>();
            legsAnimation.characterCamera = cam;
            controller.OrbitCamera = cam;

            if (world) {
                WorldNPCAI ai = npc.GetComponent<WorldNPCAI>();
                // if (ai)
                ai?.Initialize(storeType);
            } else {
                CivilianNPCAI ai = npc.GetComponent<CivilianNPCAI>();
                ai?.Initialize();
            }

            // should be part of apply state?
            motor.SetPosition(destination, bypassInterpolation: true);
            ApplyNPCState(template, npc);
            return npc;
        } else {
            return null;
        }
    }
    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.ApplyState(npcObject);
    }
}
