using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskSetKey<T> : TaskNode {
        Func<T> supplier;
        string key;
        public TaskSetKey(string key, Func<T> supplier) : base() {
            this.key = key;
            this.supplier = supplier;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            parent.SetData(key, supplier());
            // Debug.Log($"setting {key}");
            return TaskState.success;
        }
    }

}
