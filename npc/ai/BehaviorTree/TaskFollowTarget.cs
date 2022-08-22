using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskFollowTarget : TaskNode {
        public static readonly string FOLLOW_TARGET_KEY = "followTargetPosition";

        public enum HeadBehavior { normal, left, right, rear }
        // private static readonly float SPACING_DISTANCE = 1.5f;
        // private static readonly float REPATH_DISTANCE = 3.5f;
        TaskMoveToKey taskMoveToKey;
        public HeadBehavior headBehavior;
        public float speedCoefficient = 1f;
        public Transform targetTransform;
        public Transform transform;

        public TaskFollowTarget(Transform transform, GameObject target, HeadBehavior headBehavior = HeadBehavior.normal) : base() {
            this.targetTransform = target.transform;
            this.transform = transform;
            this.headBehavior = headBehavior;
            taskMoveToKey = new TaskMoveToKey(transform, FOLLOW_TARGET_KEY);
            taskMoveToKey.SetData(FOLLOW_TARGET_KEY, targetTransform.position);
        }

        public override TaskState DoEvaluate(ref PlayerInput input) {

            // TODO: follow behind.
            taskMoveToKey.SetData(FOLLOW_TARGET_KEY, targetTransform.position);
            taskMoveToKey.Evaluate(ref input);

            // set head look direction
            Vector3 baseLookDirection = input.moveDirection;
            float headSwivelOffset = 0;
            if (headBehavior == HeadBehavior.left) {
                headSwivelOffset = 45f;
            } else if (headBehavior == HeadBehavior.right) {
                headSwivelOffset = -45f;
            }
            Vector3 lookDirection = baseLookDirection;
            lookDirection = Quaternion.AngleAxis(headSwivelOffset, Vector3.up) * lookDirection;
            input.lookAtDirection = lookDirection;

            return TaskState.running;

            // set movement
            // Vector3 displacement = targetTransform.position - transform.position;
            // float distance = displacement.magnitude;
            // if (distance > REPATH_DISTANCE || Mathf.Abs(displacement.y) > 0.2f) {
            //     taskMoveToKey.Evaluate(ref input);
            //     return TaskState.running;
            // } else if (distance > SPACING_DISTANCE) {
            //     Vector3 direction = targetTransform.position - transform.position;
            //     inputVector = direction;
            // } else if (distance < SPACING_DISTANCE) {
            //     Vector3 direction = targetTransform.position - transform.position;
            //     inputVector = -1f * direction;
            // }
            // inputVector.y = 0;
            // input.moveDirection = speedCoefficient * inputVector.normalized;


            // PlayerInput dummyInput = new PlayerInput();
            // taskMoveToKey.Evaluate(ref dummyInput);

            // return TaskState.running;
        }
    }

}