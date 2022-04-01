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
            //  reloadCountDown -= Time.deltaTime;
            //     if (reloadCountDown <= 0)
            //         ChangeState(State.approach);
            bool reload = false;
            if (gunHandler.state == GunHandler.GunState.reloading) {
                gunHandler.ClipIn();
                gunHandler.StopReload();
            } else {
                reload = gunHandler.gunInstance.clip <= 0;
                // if (reload) {
                // ChangeState(State.reload);
                // reloadCountDown = 1f;
                // }
            }
            input.reload = reload;
            Debug.Log($"reloading {reload}");
            return TaskState.running;
        }
    }

}
