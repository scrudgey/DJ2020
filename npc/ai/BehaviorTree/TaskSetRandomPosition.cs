using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskSetRandomPosition : TaskNode {
        Func<Vector3> supplier;
        string key;
        public TaskSetRandomPosition(string key, Func<Vector3> supplier) : base() {
            this.key = key;
            this.supplier = supplier;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            parent.SetData(key, supplier());
            Debug.Log($"setting {key}");
            return TaskState.success;
        }
    }

}
