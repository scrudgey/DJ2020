using System;
namespace AI {
    public class TaskConditional : TaskNode {
        Func<bool> test;
        public TaskConditional(Func<bool> test) : base() {
            this.test = test;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) => test() ? TaskState.success : TaskState.failure;

    }
}