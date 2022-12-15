using AI;
using UnityEngine;
using UnityEngine.AI;


public class ReportGunshotsState : SphereControlState {
    static readonly public string DAMAGE_SOURCE_KEY = "damageSourceKey";
    static readonly public string COVER_POSITION_KEY = "coverPositionKey";
    readonly float ROUTINE_TIMEOUT = 60f;
    float changeStateCountDown;
    private TaskNode rootTaskNode;
    private SpeechTextController speechTextController;

    public ReportGunshotsState(SphereRobotAI ai, SpeechTextController speechTextController, NoiseComponent noise) : base(ai) {
        this.speechTextController = speechTextController;
        Vector3 damageSourcePosition = noise.transform.position;
        Vector3 coverPosition = (noise.transform.position - ai.transform.position).normalized;// * -5f;
        coverPosition.y = ai.transform.position.y;
        coverPosition *= -5f;
        coverPosition += ai.transform.position;
        SetupRootNode(initialPause: 2f);
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
        rootTaskNode.SetData(COVER_POSITION_KEY, coverPosition);
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode(float initialPause = 1f) {
        // TODO: write better code here

        string speechText = "HQ, report! Shots fired, investigating.";
        SuspicionRecord intruderRecord = new SuspicionRecord {
            content = "gunshots reported",
            maxLifetime = 120,
            lifetime = 120,
            suspiciousness = Suspiciousness.suspicious
        };
        HQReport report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = true,
            locationOfLastDisturbance = owner.getLocationOfInterest(),
            timeOfLastContact = Time.time,
            lifetime = 6f,
            speechText = speechText,
            suspicionRecord = intruderRecord
        };

        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = DAMAGE_SOURCE_KEY,
                useKey = true,
                reorient = true
            }, initialPause),
            new Selector(
                new TaskConditional(() => GameManager.I.isAlarmRadioInProgress(owner.gameObject)),
                new TaskConditional(() => GameManager.I.levelHQTerminal() == null),
                new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report)
            )
        );

    }

    public override PlayerInput Update(ref PlayerInput input) {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.StateFinished(this);
        }
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        } else if (result == TaskState.failure) {
            owner.StateFinished(this);
        }
        return input;
    }
}