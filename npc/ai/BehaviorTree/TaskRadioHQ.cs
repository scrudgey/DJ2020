using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskRadioHQ : TaskNode {
        AlertHandler alertHandler;
        bool started;
        bool stopped;
        float timer;
        float totalDuration = 4f;
        public TaskRadioHQ(AlertHandler alertHandler) : base() {
            this.alertHandler = alertHandler;
            started = false;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (!started) {
                alertHandler.ShowRadio();
                started = true;
            }
            timer += Time.deltaTime;
            if (timer < totalDuration) {
                return TaskState.running;
            } else {
                if (!stopped) {
                    alertHandler.HideRadio();
                    stopped = true;
                }
                return TaskState.success;
            }
        }
    }

}