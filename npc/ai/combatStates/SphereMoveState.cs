using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereMoveState : SphereControlState {
    public static readonly string RANDOM_POSITION_KEY = "randomPosition";
    private TaskNode rootTaskNode;
    private SphereCollider patrolZone;
    CharacterController characterController;
    public SphereMoveState(SphereRobotAI ai, SphereCollider sphere, CharacterController characterController) : base(ai) {
        this.patrolZone = sphere;
        this.characterController = characterController;
    }

    public override void Enter() {
        base.Enter();
        SetupRootNode();
        rootTaskNode.SetData(RANDOM_POSITION_KEY, randomPoint());
    }
    void SetupRootNode() {
        rootTaskNode = new TaskRepeaterDecorator(new Sequence(
            new TaskMoveToKey(owner.transform, RANDOM_POSITION_KEY, owner.physicalKeys, characterController),
            new TaskTimerDectorator(2f),
            new TaskSetKey<Vector3>(RANDOM_POSITION_KEY, randomPoint)
        ));
    }
    public Vector3 randomPoint() {
        return patrolZone.radius * UnityEngine.Random.insideUnitSphere + patrolZone.center;
    }
    public override PlayerInput Update(ref PlayerInput input) {
        rootTaskNode.Evaluate(ref input);
        return input;
    }
}