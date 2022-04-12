using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskLookAtKey : TaskNode {
        string key;
        Vector3 lookAtPoint;
        float repathTimer;
        float repathInterval = 1f;
        public TaskLookAtKey(string key) : base() { this.key = key; SetDestination(); }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            repathTimer += Time.deltaTime;
            if (repathTimer > repathInterval) {
                repathTimer = 0f;
                SetDestination();
            }
            input.lookAtDirection = lookAtPoint;
            // TODO: return complete if looking in correct direction
            return TaskState.running;
        }

        void SetDestination() {
            object keyObj = GetData(key);
            // Debug.Log($"fetched {key} {keyObj}");
            if (keyObj == null)
                return;
            Vector3 target = (Vector3)keyObj;
        }
    }

}