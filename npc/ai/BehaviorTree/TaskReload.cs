using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskReload : TaskNode {
        GunHandler gunHandler;
        public TaskReload(GunHandler gunHandler) : base() {
            this.gunHandler = gunHandler;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            input.reload = gunHandler.gunInstance.clip <= 0 && gunHandler.state != GunHandler.GunState.reloading && gunHandler.state != GunHandler.GunState.racking;
            return TaskState.running;
        }
    }

}
