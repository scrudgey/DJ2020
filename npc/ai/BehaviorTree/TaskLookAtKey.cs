using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskLookAt : TaskNode {
        public enum HeadBehavior { normal, casual, search }
        public HeadBehavior headBehavior;
        public enum LookType { position, direction }
        public LookType lookType;
        public string key;
        public Vector3 lookAtPoint;
        public bool useKey;
        public bool reorient;
        float repathTimer;
        float repathInterval = 1f;
        public float headSwivelOffset;
        private Transform transform;
        public TaskLookAt(Transform transform) : base() {
            this.transform = transform;
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


            Vector3 baseLookDirection = lookType switch {
                LookType.position => lookAtPoint - transform.position,
                LookType.direction => lookAtPoint,
                _ => lookAtPoint
            };

            if (headBehavior == HeadBehavior.casual) {
                // TODO: abstract out to some equivalent of an easing function
                headSwivelOffset = 45f * Mathf.Sin(Time.time);
            } else if (headBehavior == HeadBehavior.search) {
                headSwivelOffset = 45f * Mathf.Sin(Time.time * 2f);
            }
            Vector3 lookDirection = Quaternion.AngleAxis(headSwivelOffset, Vector3.up) * baseLookDirection;

            if (lookType == LookType.position) {
                input.lookAtPosition = transform.position + lookDirection;
                if (reorient) {
                    input.orientTowardPoint = transform.position + lookDirection;
                    input.orientTowardPoint.y = 0;
                }
            } else if (lookType == LookType.direction) {
                input.lookAtDirection = lookDirection;
                if (reorient) {
                    input.orientTowardDirection = lookDirection;
                    input.orientTowardDirection.y = 0;
                }
            }
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
            lookAtPoint = target;
        }
    }

}