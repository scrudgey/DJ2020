using AI;
using UnityEngine;
using UnityEngine.AI;


public class ReactToAttackState : SphereControlState {
    public enum AttackType { none, damage, gunshots }
    AttackType type;
    static readonly public string DAMAGE_SOURCE_KEY = "damageSourceKey";
    static readonly public string COVER_POSITION_KEY = "coverPositionKey";
    readonly float ROUTINE_TIMEOUT = 60f;
    float changeStateCountDown;
    private TaskNode rootTaskNode;
    private SpeechTextController speechTextController;

    public ReactToAttackState(SphereRobotAI ai, SpeechTextController speechTextController, Damage damage, float initialPause = 2f) : base(ai) {
        this.speechTextController = speechTextController;
        this.type = AttackType.damage;
        Vector3 damageSourcePosition = ai.transform.position + -10f * damage.direction;
        Vector3 coverPosition = ai.transform.position + 10f * damage.direction;
        SetupRootNode(initialPause: initialPause); // enough to time out hitstun
        rootTaskNode.SetData(DAMAGE_SOURCE_KEY, damageSourcePosition);
        rootTaskNode.SetData(COVER_POSITION_KEY, coverPosition);
    }
    public ReactToAttackState(SphereRobotAI ai, SpeechTextController speechTextController, NoiseComponent noise) : base(ai) {
        this.speechTextController = speechTextController;
        this.type = AttackType.gunshots;
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
        LevelState levelData = GameManager.I.gameData.levelState;
        if (GameManager.I.levelHQTerminal() != null && !levelData.anyAlarmActive()) {
            string speechText = type switch {
                AttackType.damage => "HQ respond! Taking fire!",
                AttackType.gunshots => "HQ respond! Shots fired!",
                _ => "HQ respond! Activate building alarm!"
            };
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
                    new TaskMoveToKey(owner.transform, COVER_POSITION_KEY) {
                        headBehavior = TaskMoveToKey.HeadBehavior.search,
                        speedCoefficient = 2f
                    },
                    new TaskSucceed()
                ),
                new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                    lookType = TaskLookAt.LookType.position,
                    key = DAMAGE_SOURCE_KEY,
                    useKey = true,
                    reorient = true
                }, 1.5f),
                new Selector(
                    new TaskConditional(() => GameManager.I.isAlarmRadioInProgress(owner.gameObject)),
                    new TaskConditional(() => GameManager.I.levelHQTerminal() == null),
                    new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report)
                )
            );
        } else {
            // TODO: smarter behavior here. sometimes we want to run toward the firefight.
            rootTaskNode = new Sequence(
                    new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                        lookType = TaskLookAt.LookType.position,
                        key = DAMAGE_SOURCE_KEY,
                        useKey = true,
                        reorient = true
                    }, initialPause),
                    new TaskMoveToKey(owner.transform, COVER_POSITION_KEY) {
                        headBehavior = TaskMoveToKey.HeadBehavior.search,
                        speedCoefficient = 2f
                    },
                    new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                        lookType = TaskLookAt.LookType.position,
                        key = DAMAGE_SOURCE_KEY,
                        useKey = true,
                        reorient = true
                    }, 1.5f)
                );
        }

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