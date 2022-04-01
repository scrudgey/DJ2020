using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskConditionalDecorator : TaskNode {
        TaskNode wrapped;
        Func<bool> conditional;
        public TaskConditionalDecorator(TaskNode wrapped, Func<bool> conditional) : base(wrapped) {
            this.wrapped = wrapped;
            this.conditional = conditional;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            TaskState wrapState = wrapped.Evaluate(ref input);
            if (conditional()) {
                return TaskState.success;
            } else if (wrapState == TaskState.running || wrapState == TaskState.failure) {
                return wrapState;
            } else return TaskState.running;
        }

    }

}
