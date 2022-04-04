using AI;
using UnityEngine;
using UnityEngine.AI;
public class SphereMoveRoutine : SphereControlState {
    public static readonly string RANDOM_POSITION_KEY = "randomPosition";
    private TaskNode rootTaskNode;
    private float newDestinationTimer;
    private SphereCollider patrolZone;
    int pathIndex;
    private readonly float CORNER_ARRIVAL_DISTANCE = 0.01f;
    public SphereMoveRoutine(SphereRobotAI ai, SphereCollider sphere) : base(ai) {
        this.patrolZone = sphere;
    }

    public override void Enter() {
        base.Enter();
        SetupRootNode();
        rootTaskNode.SetData(RANDOM_POSITION_KEY, randomPoint());
    }
    void SetupRootNode() {
        rootTaskNode = new Sequence(
            new TaskMoveToKey(owner.transform, RANDOM_POSITION_KEY),
            new TaskSetRandomPosition(RANDOM_POSITION_KEY, randomPoint)
        );
    }
    public Vector3 randomPoint() {
        return patrolZone.radius * UnityEngine.Random.insideUnitSphere + patrolZone.center;
    }
    public override PlayerInput Update() {
        PlayerInput input = new PlayerInput();
        rootTaskNode.Evaluate(ref input);
        return input;
    }
}