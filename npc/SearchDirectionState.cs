using AI;
using UnityEngine;
using UnityEngine.AI;


public class SearchDirectionState : SphereControlState {
    static readonly public string SEARCH_POSITION_KEY = "investigatePosition";
    readonly float ROUTINE_TIMEOUT = 20f;
    float changeStateCountDown;
    private Vector3 searchDirection;
    // Vector3 targetPoint;
    private TaskNode rootTaskNode;
    public SearchDirectionState(SphereRobotAI ai, Damage damage) : base(ai) {
        if (damage != null) {
            searchDirection = -1f * damage.direction;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode();
        rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
    }
    public SearchDirectionState(SphereRobotAI ai, NoiseComponent noise) : base(ai) {
        if (noise != null) {
            searchDirection = (noise.transform.position - owner.transform.position).normalized;
            searchDirection.y = 0;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode();
        rootTaskNode.SetData(SEARCH_POSITION_KEY, noise.transform.position);
    }
    void RandomSearchDirection() {
        searchDirection = Random.insideUnitSphere;
        searchDirection.y = 0;
        searchDirection = searchDirection.normalized;
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        owner.navMeshPath = new NavMeshPath();
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
            new TaskMoveToKey(owner.transform, SEARCH_POSITION_KEY)
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
        if (noise.data.player) {
            // searchDirection = noise.transform.position - owner.transform.position;
            searchDirection = noise.transform.position;
            changeStateCountDown = ROUTINE_TIMEOUT;
            rootTaskNode.SetData(SEARCH_POSITION_KEY, searchDirection);
        }
        // targetPoint = owner.transform.position + 5f * searchDirection;
        // SetupRootNode();    // TODO: better way of handling this
    }
}