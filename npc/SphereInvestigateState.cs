using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereInvestigateState : SphereControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    static readonly public string LAST_SEEN_PLAYER_POSITION_KEY = "lastSeenPlayerPosition";
    static readonly float AGGRESSION_THRESHOLD = 4.0f;
    static readonly float WARN_THRESHOLD = 2.0f;

    private TaskNode rootTaskNode;
    private TaskNode alertTaskNode;
    public SpeechTextController speechTextController;
    float timeSinceSawPlayer;
    Vector3 lastSeenPlayerPosition;
    TaskOpenDialogue dialogueTask;
    HQReport report;
    SpottedHighlight highlight;
    float integratedPlayerMovement;
    float totalPlayerMovement;

    public SphereInvestigateState(SphereRobotAI ai, SpottedHighlight highlight) : base(ai) {
        this.highlight = highlight;
        speechTextController = owner.GetComponentInChildren<SpeechTextController>();
        // SuspicionRecord intruderRecord = new SuspicionRecord {
        //     content = "intruder reported",
        //     maxLifetime = 120,
        //     lifetime = 120,
        //     suspiciousness = Suspiciousness.normal
        // };
        report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = true,
            locationOfLastDisturbance = owner.getLocationOfInterest(),
            timeOfLastContact = Time.time,
            lifetime = 6f,
            speechText = "HQ respond. Intruder spotted. Raise the alarm.",
            // suspicionRecord = intruderRecord
        };
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        lastSeenPlayerPosition = Vector3.zero;
        highlight.target = GameManager.I.playerObject.transform;
        integratedPlayerMovement = 0f;
    }
    public override void Exit() {
        highlight.target = null;
    }
    public bool isPlayerVisible() {
        return timeSinceSawPlayer < 0.1f;
    }
    public bool isPlayerSuspicious() {
        return integratedPlayerMovement > WARN_THRESHOLD;
    }
    public bool isPlayerAggressive() {
        return integratedPlayerMovement > AGGRESSION_THRESHOLD;
    }
    public bool seenPlayerRecently() => timeSinceSawPlayer < 5f;
    void SetupRootNode() {
        dialogueTask = new TaskOpenDialogue(owner);

        alertTaskNode = new TaskRepeaterDecorator(new TaskSucceed());

        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = LAST_SEEN_PLAYER_POSITION_KEY,
                useKey = true
            }, 0.5f),
            new Selector(
                new Sequence(
                    new TaskConditional(() => isPlayerVisible()),
                    new TaskMoveToKey(owner.transform, LAST_SEEN_PLAYER_POSITION_KEY, arrivalDistance: 1.25f) {
                        speedCoefficient = 0.5f
                    },
                    dialogueTask
                ),
                new Sequence(
                    new TaskConditional(() => seenPlayerRecently()),
                    new Sequence(
                        new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY, arrivalDistance: 1f) {
                            headBehavior = TaskMoveToKey.HeadBehavior.search
                        },
                        new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                            lookType = TaskLookAt.LookType.position,
                            key = SEARCH_POSITION_KEY,
                            useKey = true,
                            headBehavior = TaskLookAt.HeadBehavior.search
                        }, 3f)
                    )
                ),
                // TODO: conditional: standing in front of player
                new Selector(
                    new TaskConditional(() => GameManager.I.isAlarmRadioInProgress(owner.gameObject)),
                    new TaskConditional(() => GameManager.I.levelHQTerminal() == null),
                    new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report)
                )
            )
        );
    }

    public override PlayerInput Update(ref PlayerInput input) {
        timeSinceSawPlayer += Time.deltaTime;
        TaskState result = TaskState.running;
        // Debug.Log($"investigate: {timeSinceSawPlayer} {integratedPlayerMovement}");
        if (isPlayerVisible()) {
            if (integratedPlayerMovement > 0) {
                integratedPlayerMovement -= Time.deltaTime * 0.5f;
            }
        } else {
            integratedPlayerMovement += Time.deltaTime;
        }
        if (isPlayerAggressive()) {
            owner.StateFinished(this);
        } else if (isPlayerSuspicious()) {
            speechTextController.Say("<color=#ff4757>Hey! Hold it!</color>");
            result = alertTaskNode.Evaluate(ref input);
        } else {
            result = rootTaskNode.Evaluate(ref input);
        }

        if (dialogueTask.isConcluded) {
            owner.StateFinished(this);
        } else if (result == TaskState.failure || result == TaskState.success) {
            owner.StateFinished(this);
        } else if (!seenPlayerRecently()) {
            highlight.target = null;
        } else {
            highlight.target = GameManager.I.playerObject.transform;
        }
        input.lookAtPosition = lastSeenPlayerPosition;
        input.snapToLook = true;
        object keyObj = rootTaskNode.GetData(LAST_SEEN_PLAYER_POSITION_KEY);
        if (keyObj != null) {
            Vector3 target = (Vector3)keyObj;
            input.lookAtPosition = target;
        }
        return input;
    }

    public override void OnObjectPerceived(Collider other) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            if (lastSeenPlayerPosition != Vector3.zero) {
                float amountOfMotion = (other.bounds.center - lastSeenPlayerPosition).magnitude;
                integratedPlayerMovement += amountOfMotion;
                totalPlayerMovement += amountOfMotion;
            }
            // Debug.Log($"investigate: {integratedPlayerMovement}");
            timeSinceSawPlayer = 0;
            lastSeenPlayerPosition = other.bounds.center;
            rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, lastSeenPlayerPosition);
        }
    }
    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        // TODO: more detailed decision making if sound is suspicious
        if (noise.data.player) {
            if (timeSinceSawPlayer > 0.1f) {
                timeSinceSawPlayer = 100f;
                rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, noise.transform.position);
            }
            if (noise.data.suspiciousness > Suspiciousness.normal || noise.data.isFootsteps) {
                Vector3 searchDirection = noise.transform.position;
                rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
            }
        }
    }

}
