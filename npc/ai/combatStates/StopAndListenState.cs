using AI;
using UnityEngine;
using UnityEngine.AI;


public class StopAndListenState : SphereControlState {
    readonly float ROUTINE_TIMEOUT = 5;
    float changeStateCountDown;
    private TaskNode rootTaskNode;
    int numberFootstepsHeard;
    private Vector3 searchDirection;
    SphereControlState previousState;
    NoiseComponent lastNoise;
    bool suspicionHeard;
    SpeechTextController speechTextController;
    CharacterController characterController;
    public StopAndListenState(SphereRobotAI ai, SphereControlState previousState, SpeechTextController speechTextController, CharacterController characterController) : base(ai) {
        searchDirection = ai.transform.forward;
        this.previousState = previousState;
        this.speechTextController = speechTextController;
        this.characterController = characterController;
        SetupRootNode();
    }
    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        speechTextController.SayWhatWasThat();
    }
    void SetupRootNode() {

        // TODO problem: changing search direction
        Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * searchDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * searchDirection;

        rootTaskNode = new TaskTimerDectorator(new Sequence(
                   // look
                   new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                       lookType = TaskLookAt.LookType.direction,
                       lookAtPoint = searchDirection
                   }, 1f),
                   // look left
                   new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                       lookType = TaskLookAt.LookType.direction,
                       lookAtPoint = leftDirection
                   }, 1f),
                   // look right
                   new TaskTimerDectorator(new TaskLookAt(owner.transform) {
                       lookType = TaskLookAt.LookType.direction,
                       lookAtPoint = rightDirection
                   }, 1f)
               ), 3f);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.StateFinished(this, TaskState.success);
        }
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this, TaskState.success);
        }
        return input;
    }

    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        if (noise.data.player) {
            lastNoise = noise;
            searchDirection = noise.transform.position - owner.transform.position;
            SetupRootNode();

            if (noise.data.suspiciousness > Suspiciousness.normal) {
                suspicionHeard = true;
                owner.StateFinished(this, TaskState.success);
            }
            if (noise.data.isFootsteps) {
                numberFootstepsHeard += 1;
                if (numberFootstepsHeard > 2) {
                    owner.StateFinished(this, TaskState.success);
                }
            }

        } else {
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                lastNoise = noise;
                numberFootstepsHeard += 1;
            }
        }
    }
    public bool FoundSomethingSuspicious() => numberFootstepsHeard >= 2 || suspicionHeard;

    public SphereControlState getNextState() {
        if (FoundSomethingSuspicious()) {
            return new SearchDirectionState(owner, lastNoise, characterController, doIntro: false);
        } else {
            // Debug.Log("guess it was nothing");
            speechTextController.SayGuessItWasNothing();
            return previousState;
        }
    }
}