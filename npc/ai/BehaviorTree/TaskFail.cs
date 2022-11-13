using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

namespace AI {

    public class TaskFail : TaskNode {
        public TaskFail() : base() {
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            return TaskState.failure;
        }
    }

}