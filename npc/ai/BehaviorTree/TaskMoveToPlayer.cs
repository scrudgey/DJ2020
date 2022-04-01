using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskMoveToPlayer : TaskNode {
        private static readonly float CORNER_ARRIVAL_DISTANCE = 0.05f;
        public NavMeshPath navMeshPath;
        int pathIndex;
        Transform transform;
        readonly float REPATH_INTERVAL = 1f;
        float repathTimer;
        public TaskMoveToPlayer(Transform transform) : base() {
            navMeshPath = new NavMeshPath();
            pathIndex = -1;
            this.transform = transform;
            repathTimer = REPATH_INTERVAL;
            SetDestination();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0) {
                repathTimer = REPATH_INTERVAL;
                SetDestination();
            }

            if (pathIndex == -1 || navMeshPath.corners.Length == 0) {
                return TaskState.failure;
            } else if (pathIndex <= navMeshPath.corners.Length - 1) {
                Vector3 inputVector = Vector3.zero;
                Vector3 nextPoint = navMeshPath.corners[pathIndex];
                float distance = Vector3.Distance(nextPoint, transform.position);
                if (distance > CORNER_ARRIVAL_DISTANCE) {
                    Vector3 direction = nextPoint - transform.position;
                    inputVector = direction.normalized;
                } else {
                    pathIndex += 1;
                }

                inputVector.y = 0;
                input.moveDirection = inputVector;

                for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
                    Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
                }
                return TaskState.running;
            } else {
                return TaskState.success;
            }
        }

        void SetDestination() {
            object data = GetData(SphereAttackRoutine.LAST_SEEN_PLAYER_POSITION_KEY);
            if (data == null) return;
            Vector3 target = (Vector3)data;

            NavMeshHit hit = new NavMeshHit();
            if (NavMesh.SamplePosition(target, out hit, 10f, NavMesh.AllAreas)) {
                Vector3 destination = hit.position;
                NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, navMeshPath);
                pathIndex = 1;
            } else {
                Debug.Log("could not find navmeshhit");
            }
        }
    }

}