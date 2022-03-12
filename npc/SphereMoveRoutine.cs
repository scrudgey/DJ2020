using UnityEngine;
using UnityEngine.AI;
public class SphereMoveRoutine : SphereControlState {
    private NavMeshAgent navMeshAgent;
    private float newDestinationTimer;
    private SphereCollider patrolZone;
    public SphereMoveRoutine(SphereRobotAI ai, NavMeshAgent navMeshAgent, SphereCollider sphere) : base(ai) {
        this.navMeshAgent = navMeshAgent;
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
        Vector3 destination = patrolZone.radius * UnityEngine.Random.insideUnitSphere + patrolZone.center;
        destination.y = 0;
        navMeshAgent.SetDestination(destination);
        // Debug.Log($"new destination: {destination}");
    }

    public override PlayerInput getInput() {
        Vector3 inputVector = navMeshAgent.desiredVelocity.normalized;
        // Vector3 inputVector = Vector3.zero;
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
            owner.ChangeState(new SphereAttackRoutine(owner, navMeshAgent, owner.gunHandler));
        }
    }
}