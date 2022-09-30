// using System.Collections;
// using System.Collections.Generic;
// using KinematicCharacterController;
// using UnityEngine;
// using UnityEngine.AI;

// public class ScenarioManager : MonoBehaviour {
//     public GameObject NPCPrefab;
//     float respawnTimer;
//     public Transform[] spawnPoints;
//     public float respawnInterval;
//     public int numberOfEnemies;
//     public CharacterCamera characterCamera;
//     public PatrolRoute[] patrolRoutes;
//     void Start() {
//         PoolManager.I.RegisterPool(NPCPrefab, poolSize: 5);
//     }

//     void FixedUpdate() {
//         PrefabPool pool = PoolManager.I.GetPool(NPCPrefab);
//         if (pool.objectsActiveInWorld.Count < numberOfEnemies) {
//             respawnTimer += Time.deltaTime;
//             if (respawnTimer > respawnInterval) {
//                 respawnTimer -= respawnInterval;
//                 Transform spawnPoint = Toolbox.RandomFromList(spawnPoints);
//                 GameObject newNPC = pool.GetObject(spawnPoint.position);
//                 InitializeNPC(newNPC, spawnPoint.position);
//             }
//         }
//     }

//     void InitializeNPC(GameObject npc, Vector3 position) {
//         CharacterController controller = npc.GetComponentInChildren<CharacterController>();
//         SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();
//         KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
//         controller.OrbitCamera = characterCamera;
//         ai.patrolRoute = Toolbox.RandomFromList(patrolRoutes);
//         motor.SetPosition(position, bypassInterpolation: true);
//     }
// }
