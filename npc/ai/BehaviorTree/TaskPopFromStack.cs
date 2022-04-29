using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;


namespace AI {

    public class TaskPopFromStack<T> : TaskNode {
        Stack<T> stack;
        string valueKey;
        public TaskPopFromStack(Stack<T> stack, string valueKey) : base() {
            this.stack = stack;
            this.valueKey = valueKey;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            // if (stack.Count > 0) {
            return TaskState.success;
            // } else {
            //     Debug.LogWarning("empty stack failure!");
            //     return TaskState.failure;
            // }
        }
        public override void Initialize() {
            DoPopFromStack();
        }
        void DoPopFromStack() {
            // Debug.LogWarning($"popping from stack {stack.Count}");
            if (stack.Count > 0) {
                parent.SetData(valueKey, stack.Pop());
            }
        }
    }

}