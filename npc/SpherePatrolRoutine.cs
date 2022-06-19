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
        // rootTaskNode = new TaskRepeaterDecorator(new Sequence(
        //     new TaskMoveToKey(owner.transform, RANDOM_POSITION_KEY),
        //     new TaskTimerDectorator(2f),
        //     new TaskSetKey<Vector3>(RANDOM_POSITION_KEY, randomPoint)
        // ));
        Debug.Log("set up root node");

        rootTaskNode = new TaskPatrol(owner.transform, patrolRoute);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        rootTaskNode.Evaluate(ref input);
        return input;
    }
}