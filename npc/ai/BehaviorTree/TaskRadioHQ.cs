using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskRadioHQ : TaskNode {
        public enum RadioType { none, }
        AlertHandler alertHandler;
        SpeechTextController speechTextController;
        SphereRobotAI ai;
        bool started;
        bool stopped;
        HQReport report;
        public TaskRadioHQ(SphereRobotAI ai,
            SpeechTextController speechTextController,
            AlertHandler alertHandler,
            HQReport report,
            SuspicionRecord suspicionRecord = null) : base() {
            this.speechTextController = speechTextController;
            this.alertHandler = alertHandler;
            this.ai = ai;
            this.report = report;
            started = false;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (GameManager.I.levelRadioTerminal() == null) {
                GameManager.I.AddSuspicionRecord(report.suspicionRecord);
                // speechTextController.Say(report.speechText);
                return TaskState.failure;
            } else if (!started) {
                alertHandler.ShowRadio();
                started = true;
                speechTextController.Say(report.speechText);
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
