using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskTimerDectorator : TaskNode {
        TaskNode wrapped;
        private float timer;
        private float lifetime = 1f;
        // public TaskTimerDectorator(TaskNode wrapped) : base(wrapped) { this.wrapped = wrapped; }
        public TaskTimerDectorator(TaskNode wrapped, float lifetime) : base(wrapped) {
            this.wrapped = wrapped;
            this.lifetime = lifetime;
            timer = 0f;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            timer += Time.deltaTime;
            if (timer < lifetime) {
                return wrapped.Evaluate(ref input);
            } else { return TaskState.success; }
        }

    }

}
