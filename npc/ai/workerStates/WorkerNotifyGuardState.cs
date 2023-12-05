using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;
public class WorkerNotifyGuardState : WorkerNPCControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    static readonly public string LAST_SEEN_GUARD_POSITION_KEY = "lastSeenGuardPosition";
    static readonly float AGGRESSION_THRESHOLD = 4.0f;
    static readonly float WARN_THRESHOLD = 2.0f;

    private TaskNode rootTaskNode;
    private TaskNode alertTaskNode;
    public SpeechTextController speechTextController;
    float timeSinceISawGuard;
    Vector3 lastSeenGuardPosition;
    TaskNotifyGuard notifyTask;
    HQReport report;
    CharacterController characterController;
    SphereRobotAI guardAI;
    float saidHeyTimeout;
    public WorkerNotifyGuardState(WorkerNPCAI ai, CharacterController characterController, SpeechTextController speechTextController, SphereRobotAI guardAI) : base(ai) {
        this.characterController = characterController;
        this.speechTextController = speechTextController;
        this.guardAI = guardAI;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
        lastSeenGuardPosition = Vector3.zero;
        rootTaskNode.SetData(LAST_SEEN_GUARD_POSITION_KEY, guardAI.transform.position);
        alertTaskNode.SetData(LAST_SEEN_GUARD_POSITION_KEY, guardAI.transform.position);
    }

    public bool lookingAtGuard() {
        return timeSinceISawGuard < 0.1f;
    }
    public bool seenGuardRecently() => timeSinceISawGuard < 2f;

    public bool isGuardNear() {
        return Vector3.Distance(guardAI.transform.position, owner.transform.position) < 2.5f;
    }
    void SetupRootNode() {
        notifyTask = new TaskNotifyGuard(owner.gameObject, guardAI, owner);

        alertTaskNode = new Sequence(
            new TaskMoveToKey(owner.transform, LAST_SEEN_GUARD_POSITION_KEY, new HashSet<int>(), characterController, arrivalDistance: 2f) {
                headBehavior = TaskMoveToKey.HeadBehavior.search,
                speedCoefficient = 2f
            }
        );

        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                lookType = TaskLookAt.LookType.position,
                key = LAST_SEEN_GUARD_POSITION_KEY,
                useKey = true
            }, 0.25f),
            new Selector(
                new Sequence(
                    new TaskMoveToKey(owner.transform, LAST_SEEN_GUARD_POSITION_KEY, new HashSet<int>(), characterController, arrivalDistance: 2f) {
                        speedCoefficient = 2f
                    },
                    new TaskConditional(() => isGuardNear()),
                    notifyTask
                ),
                new Sequence(
                    new TaskConditional(() => seenGuardRecently()),
                    new Sequence(
                        new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY, new HashSet<int>(), characterController, arrivalDistance: 2f) {
                            headBehavior = TaskMoveToKey.HeadBehavior.search,
                            speedCoefficient = 2f
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

        timeSinceISawGuard += Time.deltaTime;
        if (saidHeyTimeout > 0) {
            saidHeyTimeout -= Time.deltaTime;
        }
        if (saidHeyTimeout <= 0) {
            speechTextController.SayPageGuard();
            saidHeyTimeout = 60f;
            NoiseData data = new NoiseData() {
                volume = 10,
                suspiciousness = Suspiciousness.suspicious,
                player = false,
                isGunshot = false,
                isFootsteps = false,
                source = owner.gameObject
            };
            Toolbox.Noise(owner.transform.position, data, owner.gameObject);
        }
        if (lookingAtGuard()) {
            result = rootTaskNode.Evaluate(ref input);
            if (result == TaskState.failure || result == TaskState.success) {
                owner.StateFinished(this);
            }
        } else {

            // if (!lookingAtGuard()) {
            result = alertTaskNode.Evaluate(ref input);
            if (result == TaskState.success) {
                owner.StateFinished(this);
            }
            // }
        }

        input.lookAtPosition = lastSeenGuardPosition;
        input.snapToLook = true;
        object keyObj = rootTaskNode.GetData(LAST_SEEN_GUARD_POSITION_KEY);
        if (keyObj != null) {
            Vector3 target = (Vector3)keyObj;
            input.lookAtPosition = target;
        }
        return input;
    }

    public override void OnObjectPerceived(Collider other) {
        if (other.transform.IsChildOf(guardAI.transform)) {
            timeSinceISawGuard = 0;
            lastSeenGuardPosition = other.transform.root.position;
            rootTaskNode.SetData(LAST_SEEN_GUARD_POSITION_KEY, lastSeenGuardPosition);
            alertTaskNode.SetData(LAST_SEEN_GUARD_POSITION_KEY, lastSeenGuardPosition);
            rootTaskNode.SetData(SEARCH_POSITION_KEY, lastSeenGuardPosition);
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