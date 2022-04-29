using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class Sequence : TaskNode {
        public Sequence() : base() { }
        public Sequence(List<TaskNode> children) : base(children) { }
        public Sequence(params TaskNode[] tasks) : base(new List<TaskNode>(tasks)) { }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            // bool anyChildIsRunning = false;
            // foreach (TaskNode node in children) {
            //     switch (node.Evaluate(ref input)) {
            //         case TaskState.failure:
            //             state = TaskState.failure;
            //             return state;
            //         case TaskState.success:
            //             continue;
            //         case TaskState.running:
            //             anyChildIsRunning = true;
            //             // return TaskState.running;
            //             continue;
            //         default:
            //             state = TaskState.success;
            //             return state;
            //     }
            // }


            // state = anyChildIsRunning ? TaskState.running : TaskState.success;
            // return state;



            bool anyChildRunning = false;
            foreach (TaskNode node in children) {
                switch (node.Evaluate(ref input)) {
                    case TaskState.failure:
                        state = TaskState.failure;
                        return state;
                    case TaskState.success:
                        continue;
                    case TaskState.running:
                        anyChildRunning = true;
                        continue;
                    default:
                        state = TaskState.success;
                        return state;
                }
            }
            state = anyChildRunning ? TaskState.running : TaskState.success;
            return state;
        }

    }
}
