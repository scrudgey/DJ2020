// using System.Collections;
// using System.Collections.Generic;
// using AI;
// using UnityEngine;
// using UnityEngine.AI;

// namespace AI {

//     public class TaskMoveToPosition : TaskNode {
//         private static readonly float CORNER_ARRIVAL_DISTANCE = 0.05f;
//         public NavMeshPath navMeshPath;
//         int pathIndex;
//         Transform transform;
//         public TaskMoveToPosition(Transform transform, Vector3 targetPosition) : base() {
//             navMeshPath = new NavMeshPath();
//             pathIndex = -1;
//             this.transform = transform;
//             SetDestination(targetPosition);
//         }
//         public override TaskState DoEvaluate(ref PlayerInput input) {
//             if (pathIndex == -1 || navMeshPath.corners.Length == 0) {
//                 return TaskState.failure;
//             } else if (pathIndex <= navMeshPath.corners.Length) {
//                 Vector3 inputVector = Vector3.zero;
//                 Vector3 nextPoint = navMeshPath.corners[pathIndex];
//                 float distance = Vector3.Distance(nextPoint, transform.position);
//                 if (distance > CORNER_ARRIVAL_DISTANCE) {
//                     Vector3 direction = nextPoint - transform.position;
//                     inputVector = direction.normalized;
//                 } else {
//                     pathIndex += 1;
//                 }

//                 inputVector.y = 0;
//                 input.moveDirection = inputVector;

//                 for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
//                     Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
//                 }
//                 return TaskState.running;
//             } else {
//                 return TaskState.success;
//             }
//         }

//         void SetDestination(Vector3 target) {
//             NavMeshHit hit = new NavMeshHit();
//             if (NavMesh.SamplePosition(target, out hit, 10f, NavMesh.AllAreas)) {
//                 Vector3 destination = hit.position;
//                 NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, navMeshPath);
//                 pathIndex = 1;
//             } else {
//                 Debug.Log("could not find navmeshhit");
//             }
//         }
//     }

// }