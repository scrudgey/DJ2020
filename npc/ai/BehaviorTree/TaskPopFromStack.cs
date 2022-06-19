using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;


namespace AI {

    public class TaskPopFromStack<T> : TaskNode {
        Stack<T> stack;
        string valueKey;
        bool emptyStack;
        public TaskPopFromStack(Stack<T> stack, string valueKey) : base() {
            this.stack = stack;
            this.valueKey = valueKey;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (!emptyStack) {
                return TaskState.success;
            } else {
                return TaskState.failure;
            }
        }
        public override void Reset() {
            base.Reset();
        }
        public override void Initialize() {
            DoPopFromStack();
        }
        void DoPopFromStack() {
            if (stack.Count > 0) {
                parent.SetData(valueKey, stack.Pop());
                emptyStack = false;
            } else {
                emptyStack = true;
            }
        }
    }

}