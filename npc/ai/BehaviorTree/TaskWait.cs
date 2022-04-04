using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskWait : TaskNode {
        private float timer;
        private float lifetime = 1f;
        public TaskWait(float lifetime) : base() {
            this.lifetime = lifetime;
            timer = 0f;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            timer += Time.deltaTime;
            Debug.Log($"waiting: {timer} {lifetime}");
            if (timer < lifetime) {
                return TaskState.running;
            } else { return TaskState.success; }
        }

        public override void Reset() {
            base.Reset();
            timer = 0f;
        }

    }

}
