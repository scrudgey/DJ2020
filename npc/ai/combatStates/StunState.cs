using AI;
using UnityEngine;
using UnityEngine.AI;


public class StunState : SphereControlState {
    readonly float ROUTINE_TIMEOUT = 1;
    private TaskNode rootTaskNode;
    public StunState(SphereRobotAI ai) : base(ai) {
        SetupRootNode();
    }
    // public override void Enter() {
    // }
    void SetupRootNode() {
        rootTaskNode = new TaskTimerDectorator(new TaskSucceed(), ROUTINE_TIMEOUT);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        // changeStateCountDown -= Time.deltaTime;
        // if (changeStateCountDown <= 0) {
        // owner.StateFinished(this);
        // }
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        return input;
    }

}