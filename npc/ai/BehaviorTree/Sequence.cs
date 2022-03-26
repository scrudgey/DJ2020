using System.Collections.Generic;

namespace AI {
    public class Sequence : TaskNode {
        public Sequence() : base() { }
        public Sequence(List<TaskNode> children) : base(children) { }
        public Sequence(params TaskNode[] tasks) {
            new Sequence(new List<TaskNode>(tasks));
        }
        public override TaskState Evaluate() {
            bool anyChildIsRunning = false;

            foreach (TaskNode node in children) {
                switch (node.Evaluate()) {
                    case TaskState.failure:
                        state = TaskState.failure;
                        return state;
                    case TaskState.success:
                        continue;
                    case TaskState.running:
                        anyChildIsRunning = true;
                        continue;
                    default:
                        state = TaskState.success;
                        return state;
                }
            }

            state = anyChildIsRunning ? TaskState.running : TaskState.success;
            return state;
        }

    }
}
