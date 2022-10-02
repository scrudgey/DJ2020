using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskMoveToKey : TaskNode {
        public enum HeadBehavior { normal, casual, search }
        public HeadBehavior headBehavior;
        float CORNER_ARRIVAL_DISTANCE = 0.15f;
        public NavMeshPath navMeshPath;
        int pathIndex;
        Transform transform;
        string key;
        float repathTimer;
        public float repathInterval = 1f;
        int navFailures = 0;
        public float headSwivelOffset;
        public float speedCoefficient = 1f;
        Vector3 baseLookDirection;

        public TaskMoveToKey(Transform transform, string key, float arrivalDistance = 0.15f) : base() {
            navMeshPath = new NavMeshPath();
            pathIndex = -1;
            this.transform = transform;
            this.key = key;
            this.CORNER_ARRIVAL_DISTANCE = arrivalDistance;
            SetDestination();
        }
        public override void Initialize() {
            SetDestination();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (navFailures >= 2) {
                return TaskState.failure;
            }
            if (repathTimer > repathInterval) {
                repathTimer = 0f;
                SetDestination();
            }
            repathTimer += Time.deltaTime;

            if (headBehavior == HeadBehavior.casual) {
                // TODO: abstract out to some equivalent of an easing function
                headSwivelOffset = 45f * Mathf.Sin(Time.time);
            } else if (headBehavior == HeadBehavior.search) {
                headSwivelOffset = 45f * Mathf.Sin(Time.time * 2f);
            }
            Vector3 lookDirection = baseLookDirection;
            lookDirection = Quaternion.AngleAxis(headSwivelOffset, Vector3.up) * lookDirection;
            input.lookAtDirection = lookDirection;

            if (pathIndex == -1 || navMeshPath.corners.Length == 0) {
                return TaskState.running;
            } else if (pathIndex <= navMeshPath.corners.Length - 1) {
                Vector3 inputVector = Vector3.zero;
                Vector3 nextPoint = navMeshPath.corners[pathIndex];
                float distance = Vector3.Distance(nextPoint, transform.position);
                if (distance > CORNER_ARRIVAL_DISTANCE) {
                    Vector3 direction = nextPoint - transform.position;
                    inputVector = direction;
                } else {
                    pathIndex += 1;
                }

                inputVector.y = 0;
                baseLookDirection = inputVector;
                input.moveDirection = speedCoefficient * inputVector.normalized;
                for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
                    Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
                }
                return TaskState.running;
            } else {
                return TaskState.success;
            }
        }

        public void SetDestination() {
            NavMeshHit hit = new NavMeshHit();
            object keyObj = GetData(key);
            if (keyObj == null)
                return;
            Vector3 target = (Vector3)keyObj;
            if (NavMesh.SamplePosition(target, out hit, 10f, NavMesh.AllAreas)) {
                Vector3 destination = hit.position;
                NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, navMeshPath);
                pathIndex = 1;
            } else if (NavMesh.SamplePosition(target, out hit, 100f, NavMesh.AllAreas)) {
                Vector3 destination = hit.position;
                NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, navMeshPath);
                pathIndex = 1;
            } else {
                Debug.LogWarning($"could not find navmeshhit for {target}");
                navFailures += 1;
            }
        }
        public override void Reset() {
            base.Reset();
            SetDestination();
        }
    }

}