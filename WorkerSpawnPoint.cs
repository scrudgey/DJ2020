using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class WorkerSpawnPoint : MonoBehaviour {

    public NPCTemplate myTemplate;
    public Transform guardPoint;
    public Transform lookAtPoint;
    PrefabPool NPCPool;
    void Start() {
        InitializePools();
    }
    void InitializePools() {
        NPCPool = PoolManager.I?.RegisterPool("prefabs/WorkerNPC", poolSize: 10);
    }
    public GameObject SpawnNPC(NPCTemplate template, bool useSpawnEffect = true) {
        if (NPCPool == null) {
            InitializePools();
        }
        GameObject npc = NPCPool.GetObject(transform.position);

        PatrolRoute route = Toolbox.RandomFromList(GameObject.FindObjectsOfType<PatrolRoute>());
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();

        WorkerNPCAI ai = npc.GetComponentInChildren<WorkerNPCAI>();
        LegsAnimation legsAnimation = npc.GetComponentInChildren<LegsAnimation>();
        legsAnimation.characterCamera = cam;
        controller.OrbitCamera = cam;

        ai.lookAtPoint = lookAtPoint;
        ai.guardPoint = guardPoint;

        motor.SetPosition(transform.position, bypassInterpolation: true);
        ApplyNPCState(template, npc);

        ai.Initialize();
        return npc;
    }
    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.activeGun = 0;
        state.ApplyState(npcObject);
    }

    public GameObject SpawnTemplated() {
        return SpawnNPC(myTemplate, useSpawnEffect: false);
    }
}
