using AI;
using UnityEngine;
using UnityEngine.AI;


public class SearchDirectionState : SphereControlState {
    int pathIndex;
    private readonly float CORNER_ARRIVAL_DISTANCE = 0.01f;
    readonly float ROUTINE_TIMEOUT = 20f;
    float changeStateCountDown;
    private Vector3 searchDirection;
    Vector3 targetPoint;
    LoHi retargetInterval = new LoHi(0.5f, 1f);
    private TaskNode rootTaskNode;
    public SearchDirectionState(SphereRobotAI ai, Damage damage) : base(ai) {
        if (damage != null) {
            searchDirection = -1f * damage.direction;
        } else {
            RandomSearchDirection();
        }
    }
    public SearchDirectionState(SphereRobotAI ai, NoiseComponent noise) : base(ai) {
        if (noise != null) {
            searchDirection = (noise.transform.position - owner.transform.position).normalized;
            searchDirection.y = 0;
            // Debug.Log($"search direction: {searchDirection}");
        } else {
            RandomSearchDirection();
        }
    }
    void RandomSearchDirection() {
        searchDirection = Random.insideUnitSphere;
        searchDirection.y = 0;
        searchDirection = searchDirection.normalized;
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        owner.navMeshPath = new NavMeshPath();
        targetPoint = owner.transform.position + 5f * searchDirection;
        SetupRootNode();
    }

    void SetupRootNode() {
        // TODO problem: changing search direction
        Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * searchDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * searchDirection;
        rootTaskNode = new Sequence(
            new TaskTimerDectorator(new Sequence(
                // look
                new TaskTimerDectorator(new TaskLookInDirection(searchDirection), 1f),
                // look left
                new TaskTimerDectorator(new TaskLookInDirection(leftDirection), 1f),
                // look right
                new TaskTimerDectorator(new TaskLookInDirection(rightDirection), 1f)
            ), 3f),
            new TaskMoveToPosition(owner.transform, targetPoint)
        );
    }

    public override PlayerInput Update() {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.RoutineFinished(this);
        }
        PlayerInput input = new PlayerInput();
        rootTaskNode.Evaluate(ref input);
        return input;
    }

    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        searchDirection = noise.transform.position - owner.transform.position;

        changeStateCountDown = ROUTINE_TIMEOUT;

        targetPoint = owner.transform.position + 5f * searchDirection;
        SetupRootNode();
    }
}