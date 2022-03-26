using AI;
using UnityEngine;
using UnityEngine.AI;


public class SearchDirectionState : SphereControlState {
    int pathIndex;
    private readonly float CORNER_ARRIVAL_DISTANCE = 0.01f;
    readonly float ROUTINE_TIMEOUT = 10f;
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
    }
    public SearchDirectionState(SphereRobotAI ai, NoiseComponent noise) : base(ai) {
        if (noise != null) {
            searchDirection = noise.transform.position - owner.transform.position;
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
        initialPosition = owner.transform.position;

        owner.navMeshPath = new NavMeshPath();
        targetPoint = initialPosition + searchDirection;
        // retargetTimer = retargetInterval.Random();
    }

    void SetupRootNode() {
        // TODO problem: changing search direction
        // TODO problem: null child
        rootTaskNode = new Sequence(
        // look left
         new TaskTimerDectorator(
            new TaskLookInDirection(searchDirection)
        ),
        // look right
        new TaskTimerDectorator(
            new TaskLookInDirection(searchDirection)
        )
        );
    }

    public override void Update() {
        base.Update();
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.RoutineFinished(this);
        }

        // change target position inside code
        // increase search radius gradually
        // move, eventually

        // retargetTimer -= Time.deltaTime;

        // if (retargetTimer <= 0) {
        //     retargetTimer = retargetInterval.Random();

        Vector3 jitterPoint = Random.insideUnitSphere * 0.5f;
        jitterPoint.y = 0;

        targetPoint = initialPosition + searchDirection + jitterPoint;
        // }
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

    public override PlayerInput getInput() {
        Vector3 inputVector = Vector3.zero;
        if (pathIndex <= owner.navMeshPath.corners.Length - 1) {
            Vector3 nextPoint = owner.navMeshPath.corners[pathIndex];
            float distance = Vector3.Distance(nextPoint, owner.transform.position);
            if (distance > CORNER_ARRIVAL_DISTANCE) {
                Vector3 direction = nextPoint - owner.transform.position;
                inputVector = direction.normalized;
                inputVector.y = 0;
            } else {
                pathIndex += 1;
            }
        }

        // if (slewTime > 0)
        // inputVector = Vector3.zero;

        return new PlayerInput() {
            inputMode = GameManager.I.inputMode,
            MoveAxisForward = 0f,
            MoveAxisRight = 0f,
            CameraRotation = Quaternion.identity,
            JumpDown = false,
            jumpHeld = false,
            jumpReleased = false,
            CrouchDown = false,
            runDown = false,
            Fire = new PlayerInput.FireInputs(),
            reload = false,
            selectgun = -1,
            actionButtonPressed = false,
            incrementItem = 0,
            useItem = false,
            incrementOverlay = 0,
            rotateCameraRightPressedThisFrame = false,
            rotateCameraLeftPressedThisFrame = false,
            moveDirection = inputVector,
            lookAtPoint = targetPoint
        };
    }

    public override void OnNoiseHeard(NoiseComponent noise) {
        base.OnNoiseHeard(noise);
        initialPosition = owner.transform.position;
        searchDirection = noise.transform.position - owner.transform.position;

        changeStateCountDown = ROUTINE_TIMEOUT;

        targetPoint = initialPosition + searchDirection;
        // retargetTimer = retargetInterval.Random();
    }

}