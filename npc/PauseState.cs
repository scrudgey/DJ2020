using AI;
using UnityEngine;
using UnityEngine.AI;
public class PauseState : SphereControlState {
    // float ROUTINE_TIMEOUT = 60f;
    float changeStateCountDown;
    // private TaskNode rootTaskNode;
    public SphereControlState nextState;

    public PauseState(SphereRobotAI ai, SphereControlState nextState, float pauseTime) : base(ai) {
        changeStateCountDown = pauseTime;
        Debug.Log($"start pause state: {changeStateCountDown}");
        this.nextState = nextState;
    }
    public override PlayerInput Update(ref PlayerInput input) {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            Debug.Log("change state countdown");
            owner.StateFinished(this);
        }
        return input;
    }
    public override void Exit() {
        Debug.Log("leaving pause state");
    }
}
