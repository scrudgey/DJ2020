using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskPatrol : TaskNode {
        static readonly string NAV_POINT_KEY = "nav_point_key";
        TaskNode rootNode;
        Transform transform;
        Stack<Vector3> navPoints;
        PatrolRoute patrolRoute;
        public TaskPatrol(Transform transform, PatrolRoute patrolRoute) : base() {
            this.patrolRoute = patrolRoute;
            this.transform = transform;
            navPoints = new Stack<Vector3>(patrolRoute.points.Select((transform) => transform.position));

            this.rootNode = new TaskUntilFailRepeater(new Sequence(
                new TaskPopFromStack<Vector3>(navPoints, NAV_POINT_KEY),
                new TaskMoveToKey(transform, NAV_POINT_KEY)
            ));
            this.rootNode.SetData(NAV_POINT_KEY, navPoints.Peek());
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (navPoints.Count == 0) {
                navPoints = new Stack<Vector3>(patrolRoute.points.Select((transform) => transform.position));
                this.rootNode = new TaskUntilFailRepeater(new Sequence(
                    new TaskPopFromStack<Vector3>(navPoints, NAV_POINT_KEY),
                    new TaskMoveToKey(transform, NAV_POINT_KEY)
                ));
                this.rootNode.SetData(NAV_POINT_KEY, navPoints.Peek());
            }
            // this.rootNode.SetData(NAV_POINT_KEY, navPoints.Peek());
            return rootNode.DoEvaluate(ref input);
        }
    }

}