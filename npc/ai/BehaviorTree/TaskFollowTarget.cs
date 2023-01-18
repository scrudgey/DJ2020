using System.Collections;
using System.Collections.Generic;
using AI;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.AI;
namespace AI {

    public class TaskFollowTarget : TaskNode {
        public static readonly string FOLLOW_TARGET_KEY = "followTargetPosition";
        public enum HeadBehavior { normal, left, right, rear }
        TaskMoveToKey taskMoveToKey;
        public HeadBehavior headBehavior;
        public float speedCoefficient = 1f;
        public CharacterController targetController;
        public Transform targetTransform;
        public Transform transform;
        LinkedList<Vector3> positions;
        HashSet<int> keyIds;
        CharacterController characterController;
        private static readonly float POINT_SPACING = 0.8f;
        public override void Initialize() {
            positions = new LinkedList<Vector3>();
            SetFollowPoint();
        }
        public TaskFollowTarget(Transform transform, GameObject target, HashSet<int> keyIds, CharacterController characterController, HeadBehavior headBehavior = HeadBehavior.normal) : base() {
            this.targetTransform = target.transform;
            this.transform = transform;
            this.headBehavior = headBehavior;
            this.characterController = characterController;
            this.targetController = target.GetComponent<CharacterController>();
            this.keyIds = keyIds;
            taskMoveToKey = new TaskMoveToKey(transform, FOLLOW_TARGET_KEY, keyIds, characterController);
            taskMoveToKey.SetData(FOLLOW_TARGET_KEY, targetTransform.position);
        }

        public override TaskState DoEvaluate(ref PlayerInput input) {
            taskMoveToKey.speedCoefficient = targetController.Motor.Velocity.magnitude / targetController.MaxStableMoveSpeed;
            SetFollowPoint();
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
        }

        void SetFollowPoint() {
            if (positions.Count == 0) {
                positions.AddFirst(targetTransform.position);
            }
            Vector3 topPosition = positions.First.Value;
            if (Vector3.Distance(topPosition, targetTransform.position) > POINT_SPACING) {
                positions.AddFirst(targetTransform.position);
                if (positions.Count > 3) {
                    positions.RemoveLast();
                }
                Vector3 lastPosition = positions.Last.Value;
                taskMoveToKey.SetData(FOLLOW_TARGET_KEY, lastPosition);
                taskMoveToKey.SetDestination();
            }
        }
    }

}