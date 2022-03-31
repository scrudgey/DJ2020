using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskLookInDirection : TaskNode {
        // Vector3 lookAtPoint;
        Vector3 lookAtDirection;
        public TaskLookInDirection(Vector3 lookAtDirection) : base() { this.lookAtDirection = lookAtDirection; }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            input.lookAtDirection = lookAtDirection;
            // TODO: return complete if looking in correct direction
            return TaskState.running;
        }
    }

}