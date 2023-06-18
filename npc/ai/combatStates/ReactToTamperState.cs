using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;

public class ReactToTamperState : SphereControlState {
    static readonly public string CORPSE_KEY = "corpsePosition";

    private TaskNode rootTaskNode;
    private SpeechTextController speechTextController;
    TaskMoveToKey moveTask;
    TamperEvidence evidence;
    bool inPosition;
    // SuspicionRecord record;
    CharacterController characterController;
    public ReactToTamperState(SphereRobotAI ai, TamperEvidence evidence, SpeechTextController speechTextController, CharacterController characterController) : base(ai) {
        this.evidence = evidence;
        this.speechTextController = speechTextController;
        this.characterController = characterController;
        // record = SuspicionRecord.bodySuspicion();
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        rootTaskNode.SetData(CORPSE_KEY, evidence.transform.position);
    }
    void SetupRootNode() {
        moveTask = new TaskMoveToKey(owner.transform, CORPSE_KEY, owner.physicalKeys, characterController, arrivalDistance: 2f) {
            headBehavior = TaskMoveToKey.HeadBehavior.normal,
            speedCoefficient = 0.75f
        };
        HQReport report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = HQReport.AlarmChange.noChange,
            locationOfLastDisturbance = evidence.transform.position,
            timeOfLastContact = Time.time,
            lifetime = 5f,
            speechText = evidence.reportText,
            suspicionRecord = SuspicionRecord.tamperEvidenceSuspicion(evidence)
        };
        inPosition = false;
        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = CORPSE_KEY,
                useKey = true,
                reorient = true
            }, 2f),
            moveTask,
            new TaskLambda(() => inPosition = true),
            new Selector(
                new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report),
                new TaskTimerDectorator(3f)
            )
        );
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        bool characterstopped = owner.motor != null ? owner.motor.Velocity == Vector3.zero : true;
        if (inPosition && characterstopped) {
            input.CrouchDown = true;
            input.unCrawl = true;
            evidence.reported = true;
        }
        if (result == TaskState.success || result == TaskState.failure) {
            // GameManager.I.AddSuspicionRecord(record);
            owner.StateFinished(this);
        }
        return input;
    }
}