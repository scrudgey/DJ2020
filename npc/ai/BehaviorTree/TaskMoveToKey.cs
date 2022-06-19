using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskMoveToKey : TaskNode {
        private static readonly float CORNER_ARRIVAL_DISTANCE = 0.1f;
        public NavMeshPath navMeshPath;
        int pathIndex;
        Transform transform;
        string key;
        float repathTimer;
        float repathInterval = 1f;
        public TaskMoveToKey(Transform transform, string key) : base() {
            navMeshPath = new NavMeshPath();
            pathIndex = -1;
            this.transform = transform;
            this.key = key;
            SetDestination();
        }
        public override void Initialize() {
            SetDestination();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (repathTimer > repathInterval) {
                repathTimer = 0f;
                SetDestination();
            }
            repathTimer += Time.deltaTime;
            if (pathIndex == -1 || navMeshPath.corners.Length == 0) {
                // return TaskState.failure;
                return TaskState.running;
            } else if (pathIndex <= navMeshPath.corners.Length - 1) {
                Vector3 inputVector = Vector3.zero;
                Vector3 nextPoint = navMeshPath.corners[pathIndex];
                float distance = Vector3.Distance(nextPoint, transform.position);
                // Debug.Log($"dist:{distance}\tat point:{distance <= CORNER_ARRIVAL_DISTANCE}\tnext:{nextPoint}\tdirection:{nextPoint - transform.position}");
                if (distance > CORNER_ARRIVAL_DISTANCE) {
                    Vector3 direction = nextPoint - transform.position;
                    inputVector = direction;
                } else {
                    pathIndex += 1;
                }

                inputVector.y = 0;
                input.moveDirection = inputVector.normalized;

                // TODO: this is setting head orientation.
                input.Fire.targetData.position = transform.position + inputVector;
                for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
                    Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
                }
                return TaskState.running;
            } else {
                return TaskState.success;
            }
        }

        void SetDestination() {
            NavMeshHit hit = new NavMeshHit();
            object keyObj = GetData(key);
            if (keyObj == null)
                return;
            Vector3 target = (Vector3)keyObj;
            if (NavMesh.SamplePosition(target, out hit, 10f, NavMesh.AllAreas)) {
                Vector3 destination = hit.position;
                NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, navMeshPath);
                pathIndex = 1;
            } else {
                Debug.LogError($"could not find navmeshhit for {target}");
            }
        }
        public override void Reset() {
            base.Reset();
            SetDestination();
        }
    }

}