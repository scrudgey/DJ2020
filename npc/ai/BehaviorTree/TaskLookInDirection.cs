using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskLookInDirection : TaskNode {
        Vector3 lookAtPoint;
        Vector3 lookAtDirection;
        public TaskLookInDirection(Vector3 lookAtPoint) { this.lookAtPoint = lookAtPoint; }
        public override TaskState Evaluate() {
            return TaskState.running;
        }
        // public override bool Complete() {
        //     // TODO: compare vs. desired direction
        //     return false;
        // }
        // public override void DoUpdate() {
        //     // TODO: slew toward direction
        // }
    }

}