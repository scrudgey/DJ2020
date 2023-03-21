using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// either 

namespace AI {
    public class TaskTimerDectorator : TaskNode {
        TaskNode wrapped;
        private float timer;
        private float lifetime = 1f;
        public TaskTimerDectorator(float lifetime) : base() {
            this.lifetime = lifetime;
            timer = 0f;
        }
        public TaskTimerDectorator(TaskNode wrapped, float lifetime) : base(wrapped) {
            this.wrapped = wrapped;
            this.lifetime = lifetime;
            timer = 0f;
            this.children = new List<TaskNode> { wrapped };
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            timer += Time.deltaTime;
            if (timer < lifetime) {
                if (wrapped != null) {
                    wrapped.Evaluate(ref input);
                    return TaskState.running;
                } else {
                    return TaskState.running;
                }
            } else {
                return TaskState.success;
            }
        }
        public override void Reset() {
            base.Reset();
            timer = 0f;
        }

        public void Abort() {
            timer = lifetime;
        }
    }

}
