using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskLookAt : TaskNode {
        public enum LookType { position, direction }
        public LookType lookType;
        public string key;
        public Vector3 lookAt;
        public bool useKey;
        float repathTimer;
        float repathInterval = 1f;
        public TaskLookAt() : base() {
            if (useKey)
                SetDestination();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (useKey) {
                repathTimer += Time.deltaTime;
                if (repathTimer > repathInterval) {
                    repathTimer = 0f;
                    SetDestination();
                }
            }

            if (lookType == LookType.position) {
                input.lookAtPosition = lookAt;
            } else if (lookType == LookType.direction) {
                input.lookAtDirection = lookAt;
            }

            // TODO: return complete if looking in correct direction?
            return TaskState.running;
        }

        void SetDestination() {
            object keyObj = GetData(key);
            // Debug.Log($"fetched {key} {keyObj}");
            if (keyObj == null)
                return;
            Vector3 target = (Vector3)keyObj;
            if (target == null)
                return;
            lookAt = target;
        }
    }

}