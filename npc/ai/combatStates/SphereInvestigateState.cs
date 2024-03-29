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
    public NeoDialogueMenu.DialogueResult dialogueResult;
    float timeSinceSawPlayer;
    Vector3 lastSeenPlayerPosition;
    TaskOpenDialogue dialogueTask;
    HQReport report;
    SpottedHighlight highlight;
    float integratedPlayerMovement;
    float totalPlayerMovement;
    float saidHeyTimeout;
    CharacterController characterController;
    public SphereInvestigateState(SphereRobotAI ai, SpottedHighlight highlight, CharacterController characterController, SpeechTextController speechTextController) : base(ai) {
        this.highlight = highlight;
        this.characterController = characterController;
        this.speechTextController = speechTextController;
        report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = HQReport.AlarmChange.raiseAlarm,
            locationOfLastDisturbance = owner.getLocationOfInterest(),
            timeOfLastContact = Time.time,
            lifetime = 6f,
            speechText = "HQ respond. Intruder spotted. Raise the alarm.",
        };
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        lastSeenPlayerPosition = Vector3.zero;
        rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, GameManager.I.playerObject.transform.position);
        alertTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, GameManager.I.playerObject.transform.position);
        highlight.target = GameManager.I.playerObject.transform;
        integratedPlayerMovement = 0f;
    }
    public override void Exit() {
        highlight.target = null;
    }
    public bool lookingAtPlayer() {
        return timeSinceSawPlayer < 0.1f;
    }
    public bool seenPlayerRecently() => timeSinceSawPlayer < 2f;

    public bool isPlayerNear() {
        return Vector3.Distance(GameManager.I.playerObject.transform.position, owner.transform.position) < 2.5f;
    }
    public bool isPlayerSuspicious() {
        return integratedPlayerMovement > WARN_THRESHOLD;
    }
    public bool isPlayerAggressive() {
        return integratedPlayerMovement > AGGRESSION_THRESHOLD;
    }
    void SetupRootNode() {
        dialogueTask = new TaskOpenDialogue(owner.gameObject, owner.myCharacterInput(), HandleDialogueResult);

        alertTaskNode = new Sequence(
            new TaskMoveToKey(owner.transform, LAST_SEEN_PLAYER_POSITION_KEY, owner.physicalKeys, characterController, arrivalDistance: 2f) {
                headBehavior = TaskMoveToKey.HeadBehavior.search,
                speedCoefficient = 1.2f,
                highlight = highlight
            },
            new Selector(
                    new TaskConditional(() => GameManager.I.isAlarmRadioInProgress(owner.gameObject)),
                    new TaskConditional(() => GameManager.I.levelRadioTerminal() == null),
                    new TaskRadioHQ(owner, speechTextController, owner.alertHandler, report)
                )
        );

        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = LAST_SEEN_PLAYER_POSITION_KEY,
                useKey = true
            }, 0.5f),
            new Selector(
                new Sequence(
                    new TaskMoveToKey(owner.transform, LAST_SEEN_PLAYER_POSITION_KEY, owner.physicalKeys, characterController, arrivalDistance: 2f) {
                        speedCoefficient = 0.5f,
                        highlight = highlight
                    },
                    new TaskConditional(() => isPlayerNear()),
                    dialogueTask
                ),
                new Sequence(
                    new TaskConditional(() => seenPlayerRecently()),
                    new Sequence(
                        new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY, owner.physicalKeys, characterController, arrivalDistance: 2f) {
                            headBehavior = TaskMoveToKey.HeadBehavior.search,
                            highlight = highlight,
                        },
                        new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                            lookType = TaskLookAt.LookType.position,
                            key = SEARCH_POSITION_KEY,
                            useKey = true,
                            headBehavior = TaskLookAt.HeadBehavior.search
                        }, 0.5f)
                    )
                )
            )
        );
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = TaskState.running;
        if (lookingAtPlayer()) {
            if (integratedPlayerMovement > 0) {
                integratedPlayerMovement -= Time.deltaTime * 0.5f;
            }
        }
        timeSinceSawPlayer += Time.deltaTime;
        if (saidHeyTimeout > 0) {
            saidHeyTimeout -= Time.deltaTime;
        }

        if (isPlayerAggressive()) {
            owner.StateFinished(this, TaskState.success);
        }

        if (!isPlayerSuspicious() && lookingAtPlayer()) {
            result = rootTaskNode.Evaluate(ref input);
            if (result == TaskState.failure || result == TaskState.success) {
                owner.StateFinished(this, TaskState.success);
            }
        } else {
            if (saidHeyTimeout <= 0) {
                speechTextController.SayHoldIt();
                saidHeyTimeout = 60f;
            }
            if (!lookingAtPlayer()) {
                result = alertTaskNode.Evaluate(ref input);
                if (result == TaskState.success) {
                    owner.StateFinished(this, TaskState.success);
                }
            }
        }

        if (!seenPlayerRecently()) {
            highlight.target = null;
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

    public void HandleDialogueResult(NeoDialogueMenu.DialogueResult result) {
        // DialogueController.OnDialogueConclude -= HandleDialogueResult;
        dialogueResult = result;
        owner.StateFinished(this, TaskState.success);
    }

    public override void OnObjectPerceived(Collider other) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            if (lastSeenPlayerPosition != Vector3.zero) {
                float amountOfMotion = (other.transform.root.position - lastSeenPlayerPosition).magnitude;
                integratedPlayerMovement += amountOfMotion;
                totalPlayerMovement += amountOfMotion;
            }
            timeSinceSawPlayer = 0;
            lastSeenPlayerPosition = other.transform.root.position;
            rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, lastSeenPlayerPosition);
            alertTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, lastSeenPlayerPosition);
            rootTaskNode.SetData(SEARCH_POSITION_KEY, lastSeenPlayerPosition);
        }
    }
    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        if (noise.data.player) {
            if (noise.data.suspiciousness > Suspiciousness.normal || noise.data.isFootsteps) {
                Vector3 searchDirection = noise.transform.position;
                rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
                alertTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
            }
        }
    }

}
