using System.Collections.Generic;

namespace AI {
    public class Selector : TaskNode {
        // move through whole list until we find a running or non-failure.
        // stop on success or running.
        // if all fail, then fail
        public Selector() : base() { }
        public Selector(List<TaskNode> children) : base(children) { }
        public Selector(params TaskNode[] tasks) : base(new List<TaskNode>(tasks)) { }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            foreach (TaskNode node in children) {
                switch (node.Evaluate(ref input)) {
                    case TaskState.failure:
                        continue;
                    case TaskState.success:
                        state = TaskState.success;
                        return state;
                    case TaskState.running:
                        state = TaskState.running;
                        return state;
                    default:
                        continue;
                }
            }

            state = TaskState.failure;
            return state;
        }
    }
}
