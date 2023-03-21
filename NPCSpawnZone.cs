using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCSpawnZone : MonoBehaviour {
    public LoHi number;
    public NPCTemplate[] templates;
    public Collider zone;
    PrefabPool NPCPool;
    WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.01f);

    void Start() {
        InitializePools();
    }
    void InitializePools() {
        NPCPool = PoolManager.I?.RegisterPool("prefabs/WorldNPC", poolSize: 100);
        // effectPool = PoolManager.I?.RegisterPool(spawnEffect, poolSize: 5);
    }
    public void SpawnNPCs() {
        // StartCoroutine(SpawnRoutine());
        SpawnRoutineBlocking();
    }
    void SpawnRoutineBlocking() {
        for (int i = 0; i < number.GetRandomInsideBound(); i++) {
            NPCTemplate template = Toolbox.RandomFromList(templates);
            SpawnNPC(template);
        }
    }
    IEnumerator SpawnRoutine() {
        for (int i = 0; i < number.GetRandomInsideBound(); i++) {
            NPCTemplate template = Toolbox.RandomFromList(templates);
            SpawnNPC(template);
            yield return wait;
        }
    }

    GameObject SpawnNPC(NPCTemplate template) {
        Vector3 point = Toolbox.RandomInsideBounds(zone);
        GameObject npc = NPCPool.GetObject(point);

        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
        controller.OrbitCamera = cam;

        WorldNPCAI ai = npc.GetComponent<WorldNPCAI>();
        ai.Initialize();

        // should be part of apply state?
        motor.SetPosition(point, bypassInterpolation: true);
        ApplyNPCState(template, npc);
        return npc;
    }
    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.ApplyState(npcObject);
    }
}
