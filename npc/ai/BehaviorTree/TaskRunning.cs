using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskRunning : TaskNode {
        public TaskRunning() : base() {
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            return TaskState.running;
        }
    }

}