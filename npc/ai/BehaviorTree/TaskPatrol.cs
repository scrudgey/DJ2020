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
        TaskNode rootNode;
        Transform transform;
        Stack<Vector3> navPoints;
        PatrolRoute patrolRoute;
        HashSet<int> keyIds;
        public TaskPatrol(Transform transform, PatrolRoute patrolRoute, HashSet<int> keyIds) : base() {
            this.patrolRoute = patrolRoute;
            this.transform = transform;
            this.keyIds = keyIds;
            setupRootNode();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            TaskState result = rootNode.DoEvaluate(ref input);
            if (result == TaskState.success && navPoints.Count == 0) {
                setupRootNode();
            }
            return result;
        }

        void setupRootNode() {
            navPoints = new Stack<Vector3>(patrolRoute.points.Select((transform) => transform.position));
            this.rootNode = new TaskUntilFailRepeater(new Sequence(
                new TaskConditional(() => isDoneWaiting()),
                new TaskPopFromStack<Vector3>(navPoints, NAV_POINT_KEY),
                new TaskMoveToKey(transform, NAV_POINT_KEY, keyIds) {
                    headBehavior = TaskMoveToKey.HeadBehavior.casual,
                    speedCoefficient = 0.35f
                }
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