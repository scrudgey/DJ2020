using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public enum TaskState {
        running,
        success,
        failure
    }

    public abstract class TaskNode {
        protected TaskState state;
        public TaskNode parent;
        protected List<TaskNode> children;
        public TaskNode() {
            parent = null;
        }
        public TaskNode(TaskNode child) {
            this.children = new List<TaskNode>();
            _Attach(child);
        }
        public TaskNode(List<TaskNode> children) {
            this.children = new List<TaskNode>();
            foreach (TaskNode child in children)
                _Attach(child);
        }
        private void _Attach(TaskNode node) {
            node.parent = this;
            children.Add(node);
        }
        public abstract TaskState Evaluate(ref PlayerInput input);
    }
}
