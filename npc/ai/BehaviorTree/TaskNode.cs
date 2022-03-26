using System.Collections;
using System.Collections.Generic;

namespace AI {
    public enum TaskState {
        running,
        success,
        failure
    }

    public abstract class TaskNode {
        protected TaskState state;
        public TaskNode parent;
        protected List<TaskNode> children = new List<TaskNode>();
        public TaskNode() {
            parent = null;
        }
        public TaskNode(TaskNode child) {
            _Attach(child);
        }
        public TaskNode(List<TaskNode> children) {
            foreach (TaskNode child in children)
                _Attach(child);
        }
        private void _Attach(TaskNode node) {
            node.parent = this;
            children.Add(node);
        }
        public abstract TaskState Evaluate();
        public virtual void Update() { }
        // TODO: some sort of playerinput
    }
}
