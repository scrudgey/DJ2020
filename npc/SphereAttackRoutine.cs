using UnityEngine;
using UnityEngine.AI;
public class SphereAttackRoutine : SphereControlState {
    private NavMeshAgent navMeshAgent;
    private float newDestinationTimer;
    float repathCountDown;
    float changeStateCountDown;
    readonly float REPATH_INTERVAL = 0.1f;
    readonly float ATTACK_TIMEOUT = 10f;
    bool doShoot;
    float doShootCountdown;
    public SphereAttackRoutine(SphereRobotAI ai, NavMeshAgent navMeshAgent) : base(ai) {
        this.navMeshAgent = navMeshAgent;
    }
    public override void Enter() {
        // base.Enter();
        changeStateCountDown = ATTACK_TIMEOUT;
    }
    public override void Update() {
        repathCountDown -= Time.deltaTime;
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            Debug.Log("change state to move");
            owner.ChangeState(new SphereMoveRoutine(owner, navMeshAgent, owner.patrolZone));
        }
        if (repathCountDown <= 0) {
            repathCountDown += REPATH_INTERVAL;
            SetDestination(owner.lastSeenPlayerPosition);
        }
    }
    void SetDestination(Vector3 destination) {
        navMeshAgent.SetDestination(destination);
    }

    public override PlayerInput getInput() {
        // TODO: check on visibility and range
        Vector3 inputVector = navMeshAgent.desiredVelocity.normalized;

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
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            changeStateCountDown = ATTACK_TIMEOUT;
            doShoot = true;
        }
    }
}