using AI;
using UnityEngine;
using UnityEngine.AI;


public class StunState : SphereControlState {
    readonly float ROUTINE_TIMEOUT = 1;
    private TaskNode rootTaskNode;
    public StunState(SphereRobotAI ai) : base(ai) {
        SetupRootNode();
    }
    void SetupRootNode() {
        rootTaskNode = new TaskTimerDectorator(new TaskSucceed(), ROUTINE_TIMEOUT);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this, result);
        }
        return input;
    }

}