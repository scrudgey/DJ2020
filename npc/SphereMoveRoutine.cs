using UnityEngine;
using UnityEngine.AI;
public class SphereMoveRoutine : SphereControlState {
    private float newDestinationTimer;
    private SphereCollider patrolZone;
    int pathIndex;
    private readonly float CORNER_ARRIVAL_DISTANCE = 0.01f;
    public SphereMoveRoutine(SphereRobotAI ai, SphereCollider sphere) : base(ai) {
        this.patrolZone = sphere;
    }

    public override void Update() {
        newDestinationTimer -= Time.deltaTime;
        if (newDestinationTimer <= 0) {
            SetDestination();
            newDestinationTimer = Random.Range(2f, 15f);
        }
    }
    void SetDestination() {
        Vector3 randPoint = patrolZone.radius * UnityEngine.Random.insideUnitSphere + patrolZone.center;
        NavMeshHit hit = new NavMeshHit();
        if (NavMesh.SamplePosition(randPoint, out hit, 10f, NavMesh.AllAreas)) {
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
            moveDirection = inputVector
        };
    }

    public override void OnObjectPerceived(Collider other) {
        // Debug.DrawLine(transform.position, other.transform.position, Color.yellow, 1f);
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            Debug.Log("change state to attack");
            owner.ChangeState(new SphereAttackRoutine(owner, owner.gunHandler));
        }
    }
}