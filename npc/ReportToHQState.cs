using AI;
using UnityEngine;
using UnityEngine.AI;


public class ReportToHQState : SphereControlState {
    public enum AttackType { none, damage, gunshots }
    AttackType type;
    static readonly public string DAMAGE_SOURCE_KEY = "damageSourceKey";
    // static readonly public string COVER_POSITION_KEY = "coverPositionKey";
    readonly float ROUTINE_TIMEOUT = 60f;
    float changeStateCountDown;
    private TaskNode rootTaskNode;
    private SpeechTextController speechTextController;

    public ReportToHQState(SphereRobotAI ai, SpeechTextController speechTextController, Damage damage) : base(ai) {
        this.speechTextController = speechTextController;
        this.type = AttackType.damage;
        Vector3 damageSourcePosition = ai.transform.position + -10f * damage.direction;
        SetupRootNode(initialPause: 2f); // enough to time out hitstun
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
    }
    public ReportToHQState(SphereRobotAI ai, SpeechTextController speechTextController, NoiseComponent noise) : base(ai) {
        this.speechTextController = speechTextController;
        this.type = AttackType.gunshots;
        Vector3 damageSourcePosition = noise.transform.position;
        SetupRootNode(initialPause: 2f);
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
    }
    public ReportToHQState(SphereRobotAI ai, SpeechTextController speechTextController, Vector3 position) : base(ai) {
        this.speechTextController = speechTextController;
        this.type = AttackType.none;
        Vector3 damageSourcePosition = position;
        SetupRootNode(initialPause: 2f);
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        owner.navMeshPath = new NavMeshPath();
    }

    void SetupRootNode(float initialPause = 1f) {
        // TODO: write better code here
        LevelData levelData = GameManager.I.gameData.levelData;

        string speechText = type switch {
            AttackType.damage => "HQ respond! Taking fire!",
            AttackType.gunshots => "HQ respond! Shots fired!",
            _ => "HQ respond! Activate building alarm!"
        };

        HQReport report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = true,
            locationOfLastDisturbance = owner.getLocationOfInterest(),
            timeOfLastContact = Time.time,
            lifetime = 6f,
            speechText = speechText
        };

        rootTaskNode =
                new Selector(
                    new TaskConditional(() => GameManager.I.isAlarmRadioInProgress(owner.gameObject)),
                    new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report)
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