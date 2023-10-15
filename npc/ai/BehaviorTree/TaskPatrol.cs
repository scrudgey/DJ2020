using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskPatrol : TaskNode {
        public static readonly string NAV_POINT_KEY = "nav_point_key";
        public static readonly string WAIT_KEY = "wait_key";
        public enum PatrolType { casual, lively }
        public PatrolType type;
        TaskNode rootNode;
        Transform transform;
        Stack<Vector3> navPoints;
        PatrolRoute patrolRoute;
        HashSet<int> keyIds;
        bool reverse;
        CharacterController characterController;
        public TaskPatrol(Transform transform,
                            PatrolRoute patrolRoute,
                            PatrolType patrolType,
                            HashSet<int> keyIds,
                            CharacterController characterController) : base() {
            this.patrolRoute = patrolRoute;
            this.transform = transform;
            this.keyIds = keyIds;
            this.type = patrolType;
            this.characterController = characterController;
            setupRootNode();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            TaskState result = rootNode.DoEvaluate(ref input);
            if (result == TaskState.success && navPoints.Count == 0) {
                // if (patrolRoute.type == PatrolRoute.PatrolRouteType.pingPong)
                //     reverse = !reverse;
                setupRootNode();
            }
            return result;
        }

        void setupRootNode() {
            navPoints = new Stack<Vector3>();
            if (patrolRoute.type == PatrolRoute.PatrolRouteType.pingPong) {
                patrolRoute.points.Select((transform) => transform.position).ToList().ForEach(point => navPoints.Push(point));
                patrolRoute.points.Select((transform) => transform.position).Reverse().ToList().ForEach(point => navPoints.Push(point));
            } else {
                patrolRoute.points.Select((transform) => transform.position).ToList().ForEach(point => navPoints.Push(point));
                patrolRoute.points.Select((transform) => transform.position).ToList().ForEach(point => navPoints.Push(point));
            }

            Vector3 closestPoint = patrolRoute.points
                .Select((transform) => transform.position)
                .OrderBy(point => Vector3.Distance(point, transform.position))
                .First();

            while (navPoints.Peek() != closestPoint && navPoints.Count > 0) {
                Vector3 position = navPoints.Pop();
            }

            TaskMoveToKey taskMoveToKey = type switch {
                PatrolType.casual => new TaskMoveToKey(transform, NAV_POINT_KEY, keyIds, characterController) {
                    headBehavior = TaskMoveToKey.HeadBehavior.casual,
                    speedCoefficient = 0.35f
                },
                PatrolType.lively => new TaskMoveToKey(transform, NAV_POINT_KEY, keyIds, characterController) {
                    headBehavior = TaskMoveToKey.HeadBehavior.search,
                    speedCoefficient = 0.6f
                }
            };

            this.rootNode = new TaskUntilFailRepeater(new Sequence(
                new TaskConditional(() => isDoneWaiting()),
                new TaskPopFromStack<Vector3>(navPoints, NAV_POINT_KEY),
                taskMoveToKey
            ));
            this.rootNode.SetData(NAV_POINT_KEY, navPoints.Peek());
        }

        bool isDoneWaiting() {
            object keyObj = GetData(WAIT_KEY);
            if (keyObj == null)
                return true;
            bool waiting = (bool)keyObj;
            return !waiting;
        }
    }

}