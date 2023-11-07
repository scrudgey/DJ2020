using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AI {
    public class TaskNotifyGuard : TaskNode {
        GameObject gameObject;
        SphereRobotAI guardAI;
        WorkerNPCAI myAI;
        public TaskNotifyGuard(GameObject gameObject, SphereRobotAI guardAI, WorkerNPCAI myAI) : base() {
            this.gameObject = gameObject;
            this.guardAI = guardAI;
            this.myAI = myAI;
        }
        public override void Initialize() {
            base.Initialize();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            guardAI.TellAboutSuspiciousPlayer(myAI);
            if (myAI.someoneWasShot) {
                myAI.someoneWasShot = false;
                SuspicionRecord record = SuspicionRecord.shotSuspicion();
                GameManager.I.AddSuspicionRecord(record);
            }
            return TaskState.success;
        }

    }
}
