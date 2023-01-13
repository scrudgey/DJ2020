using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class RobotSpawnPoint : MonoBehaviour {
    public bool isStrikeTeamSpawn;
    public GameObject spawnEffect;
    PrefabPool effectPool;
    PrefabPool NPCPool;
    void Start() {
        InitializePools();
    }
    void InitializePools() {
        NPCPool = PoolManager.I?.RegisterPool("prefabs/sphereRobot", poolSize: 10);
        effectPool = PoolManager.I?.RegisterPool(spawnEffect, poolSize: 5);
    }
    public GameObject SpawnNPC(bool useSpawnEffect = true) {
        if (effectPool == null) {
            InitializePools();
        }
        effectPool.GetObject(transform.position);
        GameObject npc = NPCPool.GetObject(transform.position);

        PatrolRoute route = Toolbox.RandomFromList(GameObject.FindObjectsOfType<PatrolRoute>());

        SphereRobotController controller = npc.GetComponentInChildren<SphereRobotController>();

        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();

        ai.patrolRoute = route;
        ai.physicalKeys = new HashSet<int>(1);
        motor.SetPosition(transform.position, bypassInterpolation: true);

        return npc;
    }
}
