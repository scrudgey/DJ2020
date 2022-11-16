using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskLambda : TaskNode {
        Action lambda;
        public TaskLambda(Action lambda) : base() {
            this.lambda = lambda;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            // parent.SetData(key, supplier());
            // Debug.Log($"setting {key}");
            lambda();
            return TaskState.success;
        }
    }

}
