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

    public InvestigateCorpseState(SphereRobotAI ai, Corpse corpse, SpeechTextController speechTextController) : base(ai) {
        this.corpse = corpse;
        this.speechTextController = speechTextController;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        rootTaskNode.SetData(CORPSE_KEY, corpse.transform.position);
    }
    void SetupRootNode() {
        moveTask = new TaskMoveToKey(owner.transform, CORPSE_KEY, arrivalDistance: 1f) {
            headBehavior = TaskMoveToKey.HeadBehavior.normal,
            speedCoefficient = 0.75f
        };
        SuspicionRecord record = new SuspicionRecord {
            content = "A body was discovered",
            maxLifetime = 120,
            lifetime = 120,
            suspiciousness = Suspiciousness.suspicious
        };
        HQReport report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = true,
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
                new TaskConditional(() => GameManager.I.levelHQTerminal() == null),
                new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report)
            )
        );
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (inPosition) {
            input.CrouchDown = true;
            input.unCrawl = true;
            corpse.reported = true;
        }
        if (result == TaskState.success || result == TaskState.failure) {
            owner.StateFinished(this);
        }
        return input;
    }
}
