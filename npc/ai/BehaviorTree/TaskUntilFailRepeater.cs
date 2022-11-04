using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskUntilFailRepeater : TaskNode {
        TaskNode wrapped;
        public TaskUntilFailRepeater(TaskNode wrapped) : base(wrapped) {
            this.wrapped = wrapped;
            this.children = new List<TaskNode> { wrapped };

        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            TaskState wrapState = wrapped.DoEvaluate(ref input);
            if (wrapState == TaskState.success) {
                wrapped.Reset();
            } else if (wrapState == TaskState.failure) {
                return TaskState.success;
            }
            return TaskState.running;
        }

    }

}
