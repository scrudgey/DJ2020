using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class InvestigateCorpseState : SphereControlState {
    static readonly public string CORPSE_KEY = "corpsePosition";

    private TaskNode rootTaskNode;
    private SpeechTextController speechTextController;
    TaskMoveToKey moveTask;
    Corpse corpse;
    bool inPosition;
    SuspicionRecord record;
    CharacterController characterController;
    public InvestigateCorpseState(SphereRobotAI ai, Corpse corpse, SpeechTextController speechTextController, CharacterController characterController) : base(ai) {
        this.corpse = corpse;
        this.speechTextController = speechTextController;
        this.characterController = characterController;
        record = SuspicionRecord.bodySuspicion();
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        rootTaskNode.SetData(CORPSE_KEY, corpse.transform.position);
    }
    void SetupRootNode() {
        moveTask = new TaskMoveToKey(owner.transform, CORPSE_KEY, owner.physicalKeys, characterController, arrivalDistance: 2f) {
            headBehavior = TaskMoveToKey.HeadBehavior.normal,
            speedCoefficient = 0.75f
        };
        HQReport report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = HQReport.AlarmChange.raiseAlarm,
            locationOfLastDisturbance = corpse.transform.position,
            timeOfLastContact = Time.time,
            lifetime = 5f,
            speechText = "HQ, respond! Man down!",
            suspicionRecord = record
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
            corpse.reported = true;
        }
        if (result == TaskState.success || result == TaskState.failure) {
            GameManager.I.AddSuspicionRecord(record);
            owner.StateFinished(this, result);
        }
        return input;
    }
}
