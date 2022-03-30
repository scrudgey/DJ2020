using AI;
using UnityEngine;
using UnityEngine.AI;


public class SearchDirectionState : SphereControlState {
    int pathIndex;
    private readonly float CORNER_ARRIVAL_DISTANCE = 0.01f;
    readonly float ROUTINE_TIMEOUT = 20f;
    float changeStateCountDown;

    private Vector3 initialPosition;
    private Vector3 searchDirection;
    Vector3 targetPoint;
    // float retargetTimer; 
    LoHi retargetInterval = new LoHi(0.5f, 1f);
    private TaskNode rootTaskNode;
    public SearchDirectionState(SphereRobotAI ai, Damage damage) : base(ai) {
        if (damage != null) {
            searchDirection = -1f * damage.direction;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode();
    }
    public SearchDirectionState(SphereRobotAI ai, NoiseComponent noise) : base(ai) {
        if (noise != null) {
            searchDirection = noise.transform.position - owner.transform.position;
        } else {
            RandomSearchDirection();
        }
        SetupRootNode();
    }
    void RandomSearchDirection() {
        searchDirection = Random.insideUnitSphere;
        searchDirection.y = 0;
        searchDirection = searchDirection.normalized;
    }

    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
        initialPosition = owner.transform.position;

        owner.navMeshPath = new NavMeshPath();
        targetPoint = initialPosition + searchDirection;
        // retargetTimer = retargetInterval.Random();
    }

    void SetupRootNode() {
        // TODO problem: changing search direction
        Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * searchDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * searchDirection;
        rootTaskNode = new Sequence(
            // look
            new TaskTimerDectorator(new TaskLookInDirection(searchDirection), 2f),
            // look left
            new TaskTimerDectorator(new TaskLookInDirection(leftDirection), 2f),
            // look right
            new TaskTimerDectorator(new TaskLookInDirection(rightDirection), 2f)
        );
    }

    public override PlayerInput Update() {
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.RoutineFinished(this);
        }

        // Vector3 jitterPoint = Random.insideUnitSphere * 0.5f;
        // jitterPoint.y = 0;

        // targetPoint = initialPosition + searchDirection + jitterPoint;
        Vector3 moveDirection = Vector3.zero;
        if (pathIndex <= owner.navMeshPath.corners.Length - 1) {
            Vector3 nextPoint = owner.navMeshPath.corners[pathIndex];
            float distance = Vector3.Distance(nextPoint, owner.transform.position);
            if (distance > CORNER_ARRIVAL_DISTANCE) {
                Vector3 direction = nextPoint - owner.transform.position;
                moveDirection = direction.normalized;
                moveDirection.y = 0;
            } else {
                pathIndex += 1;
            }
        }

        PlayerInput input = new PlayerInput {
            moveDirection = moveDirection
        };
        rootTaskNode.Evaluate(ref input);
        return input;
    }
    void SetDestination(Vector3 position) {
        NavMeshHit hit = new NavMeshHit();
        if (NavMesh.SamplePosition(position, out hit, 10f, NavMesh.AllAreas)) {
            Vector3 destination = hit.position;
            NavMesh.CalculatePath(owner.transform.position, destination, NavMesh.AllAreas, owner.navMeshPath);
            pathIndex = 1;
        } else {
            Debug.Log("could not find navmeshhit");
        }
    }

    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        initialPosition = owner.transform.position;
        searchDirection = noise.transform.position - owner.transform.position;

        changeStateCountDown = ROUTINE_TIMEOUT;

        targetPoint = initialPosition + searchDirection;
        SetupRootNode();
        // retargetTimer = retargetInterval.Random();
    }
}