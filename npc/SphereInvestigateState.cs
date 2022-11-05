using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereInvestigateState : SphereControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    static readonly public string LAST_SEEN_PLAYER_POSITION_KEY = "lastSeenPlayerPosition";
    private TaskNode rootTaskNode;
    public SphereRobotSpeaker speaker;
    public SpeechTextController speechTextController;
    float timeSinceSawPlayer;
    Vector3 lastSeenPlayerPosition;
    TaskOpenDialogue dialogueTask;
    HQReport report;
    SpottedHighlight highlight;

    public SphereInvestigateState(SphereRobotAI ai, SpottedHighlight highlight) : base(ai) {
        this.highlight = highlight;
        speaker = owner.GetComponentInChildren<SphereRobotSpeaker>();
        speechTextController = owner.GetComponentInChildren<SpeechTextController>();
        report = new HQReport {
            reporter = owner.gameObject,
            desiredAlarmState = true,
            locationOfLastDisturbance = owner.getLocationOfInterest(),
            timeOfLastContact = Time.time,
            lifetime = 6f,
            speechText = "HQ respond. Intruder spotted. Raise the alarm."
        };

        if (speaker != null) {
            speaker.DoInvestigateSpeak();
        }

    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        highlight.target = GameManager.I.playerObject.transform;

    }
    public override void Exit() {
        highlight.target = null;
    }
    public bool isPlayerVisible() {
        return timeSinceSawPlayer < 0.1f;
    }
    public bool seenPlayerRecently() => timeSinceSawPlayer < 5f;
    void SetupRootNode() {
        dialogueTask = new TaskOpenDialogue(owner);

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

        // walk towards player

        // when in range, stop

        // open dialogue

        // determine outcome

        // report to hq

        // finish

        // watch for:
        //  player moves out of sight
        //      run to last position, look around, radio HQ
        //      if visual reacquired, repeat announce, move quicker, x2
        // player moves about too much
        // player takes out gun
        //  player does anything suspicious
        //      enter combat mode

    }

    public override PlayerInput Update(ref PlayerInput input) {
        timeSinceSawPlayer += Time.deltaTime;
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (dialogueTask.isConcluded) {
            owner.StateFinished(this);
        } else if (result == TaskState.failure || result == TaskState.success) {
            owner.StateFinished(this);
        }
        // TODO: set look position
        object keyObj = rootTaskNode.GetData(LAST_SEEN_PLAYER_POSITION_KEY);
        if (keyObj != null) {
            Vector3 target = (Vector3)keyObj;
            input.lookAtPosition = target;
        }
        return input;
    }

    public override void OnObjectPerceived(Collider other) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
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
