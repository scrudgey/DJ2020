using AI;
using UnityEngine;
using UnityEngine.AI;
public class SpherePatrolRoutine : SphereControlState {
    private TaskNode rootTaskNode;
    private PatrolRoute patrolRoute;
    public SpherePatrolRoutine(SphereRobotAI ai, PatrolRoute patrolRoute) : base(ai) {
        this.patrolRoute = patrolRoute;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
    }
    void SetupRootNode() {
        rootTaskNode = new TaskPatrol(owner.transform, patrolRoute);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        rootTaskNode.Evaluate(ref input);
        return input;
    }
}