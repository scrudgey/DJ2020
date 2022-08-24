using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskRadioHQ : TaskNode {
        AlertHandler alertHandler;
        SpeechTextController speechTextController;
        SphereRobotAI ai;
        bool started;
        bool stopped;
        float timer;
        float totalDuration = 4f;
        public TaskRadioHQ(SphereRobotAI ai, SpeechTextController speechTextController, AlertHandler alertHandler) : base() {
            this.speechTextController = speechTextController;
            this.alertHandler = alertHandler;
            this.ai = ai;
            started = false;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (!started) {
                alertHandler.ShowRadio();
                started = true;
                speechTextController.Say("HQ respond!");
            }
            timer += Time.deltaTime;
            if (timer < totalDuration) {
                return TaskState.running;
            } else {
                if (!stopped) {
                    alertHandler.HideRadio();
                    GameManager.I.ReportToHQ(true, disturbancePosition: ai.lastDisturbancePosition);
                    stopped = true;
                }
                return TaskState.success;
            }
        }
    }

}