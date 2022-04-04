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
        private Dictionary<string, object> dataContext = new Dictionary<string, object>();
        private bool initialized;
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
        public TaskState Evaluate(ref PlayerInput input) {
            if (!initialized) {
                Initialize();
            }
            initialized = true;
            return DoEvaluate(ref input);
        }
        public virtual void Initialize() { }
        public virtual void Reset() {
            foreach (TaskNode child in children) {
                child.Reset();
            }
        }
        public abstract TaskState DoEvaluate(ref PlayerInput input);

        public void SetData(string key, object value) {
            dataContext[key] = value;
        }

        public object GetData(string key) {
            object value = null;
            if (dataContext.TryGetValue(key, out value))
                return value;

            TaskNode node = parent;
            while (node != null) {
                value = node.GetData(key);
                if (value != null)
                    return value;
                node = node.parent;
            }
            return null;
        }

        public bool ClearData(string key) {
            if (dataContext.ContainsKey(key)) {
                dataContext.Remove(key);
                return true;
            }

            TaskNode node = parent;
            while (node != null) {
                bool cleared = node.ClearData(key);
                if (cleared)
                    return true;
                node = node.parent;
            }
            return false;
        }
    }
}
