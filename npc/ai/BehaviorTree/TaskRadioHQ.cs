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
                HQReport report = new HQReport {
                    reporter = ai.gameObject,
                    desiredAlarmState = true,
                    locationOfLastDisturbance = ai.lastDisturbancePosition,
                    timeOfLastContact = Time.time,
                    lifetime = 6f
                };
                GameManager.I.OpenReportTicket(ai.gameObject, report);
                return TaskState.running;
            } else if (!stopped) {
                bool complete = GameManager.I.ContactReportTicket(ai.gameObject);
                if (complete) {
                    alertHandler.HideRadio();
                    stopped = true;
                }
                return TaskState.running;
            } else return TaskState.success;
        }
    }
}
