using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class WorkerInvestigateState : WorkerNPCControlState {
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
    float saidHeyTimeout;
    CharacterController characterController;
    public bool gaveUp;
    public WorkerInvestigateState(WorkerNPCAI ai, CharacterController characterController, SpeechTextController speechTextController) : base(ai) {
        this.characterController = characterController;
        this.speechTextController = speechTextController;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        speechTextController.SaySpotted();
        saidHeyTimeout = 60f;
        lastSeenPlayerPosition = Vector3.zero;
        rootTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, GameManager.I.playerObject.transform.position);
        alertTaskNode.SetData(LAST_SEEN_PLAYER_POSITION_KEY, GameManager.I.playerObject.transform.position);
    }

    public bool lookingAtPlayer() {
        return timeSinceSawPlayer < 0.1f;
    }
    public bool seenPlayerRecently() => timeSinceSawPlayer < 2f;

    public bool isPlayerNear() {
        return Vector3.Distance(GameManager.I.playerObject.transform.position, owner.transform.position) < 2.5f;
    }
    void SetupRootNode() {
        dialogueTask = new TaskOpenDialogue(owner.gameObject, owner.MyCharacterInput(), HandleDialogueResult);

        alertTaskNode = new Sequence(
            new TaskMoveToKey(owner.transform, LAST_SEEN_PLAYER_POSITION_KEY, new HashSet<int>(), characterController, arrivalDistance: 2f) {
                headBehavior = TaskMoveToKey.HeadBehavior.search,
                speedCoefficient = 1.5f
            }
        );

        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = LAST_SEEN_PLAYER_POSITION_KEY,
                useKey = true
            }, 0.2f),
            new Selector(
                new Sequence(
                    new TaskMoveToKey(owner.transform, LAST_SEEN_PLAYER_POSITION_KEY, new HashSet<int>(), characterController, arrivalDistance: 1.5f) {
                        speedCoefficient = 1f
                    },
                    new TaskConditional(() => isPlayerNear()),
                    dialogueTask
                ),
                new Sequence(
                    new TaskConditional(() => seenPlayerRecently()),
                    new Sequence(
                        new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY, new HashSet<int>(), characterController, arrivalDistance: 2f) {
                            headBehavior = TaskMoveToKey.HeadBehavior.search
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
        timeSinceSawPlayer += Time.deltaTime;
        if (saidHeyTimeout > 0) {
            saidHeyTimeout -= Time.deltaTime;
        }
        if (saidHeyTimeout <= 0) {
            speechTextController.SaySpotted();
            saidHeyTimeout = 60f;
        }

        if (lookingAtPlayer()) {
            result = rootTaskNode.Evaluate(ref input);
            if (result == TaskState.failure || result == TaskState.success) {
                owner.StateFinished(this);
            }
            input.lookAtPosition = lastSeenPlayerPosition;
            input.snapToLook = true;
        } else {
            result = alertTaskNode.Evaluate(ref input);
            if (result == TaskState.success) {
                gaveUp = true;
                owner.StateFinished(this);
            }
        }


        return input;
    }

    public void HandleDialogueResult(NeoDialogueMenu.DialogueResult result) {
        dialogueResult = result;
        owner.StateFinished(this);
    }

    public override void OnObjectPerceived(Collider other) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
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