using UnityEngine;
using UnityEngine.AI;
public class SphereAttackRoutine : SphereControlState {
    readonly float REPATH_INTERVAL = 0.1f;
    readonly float ATTACK_TIMEOUT = 10f;
    readonly float SHOOT_TIMEOUT = 1f;
    readonly float SHOOT_INTERVAL = 0.1f;
    readonly float MAX_SHOOT_RANGE = 2f;
    private NavMeshAgent navMeshAgent;
    private float newDestinationTimer;
    float repathCountDown;
    float changeStateCountDown;
    public GunHandler gunHandler;
    public bool doShoot;
    float doShootCountdown;
    float shootTimer;

    public SphereAttackRoutine(SphereRobotAI ai,
                                NavMeshAgent navMeshAgent,
                                GunHandler gunHandler) : base(ai) {
        this.navMeshAgent = navMeshAgent;
        this.gunHandler = gunHandler;
    }
    public override void Enter() {
        changeStateCountDown = ATTACK_TIMEOUT;
    }
    public override void Update() {
        repathCountDown -= Time.deltaTime;
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.ChangeState(new SphereMoveRoutine(owner, navMeshAgent, owner.patrolZone));
        }
        if (repathCountDown <= 0) {
            repathCountDown += REPATH_INTERVAL;
            SetDestination(owner.lastSeenPlayerPosition);
        }
        if (doShoot) {
            doShootCountdown -= Time.deltaTime;
            shootTimer += Time.deltaTime;
            if (doShootCountdown <= 0) {
                doShoot = false;
            }
            if (shootTimer > SHOOT_INTERVAL) {
                shootTimer -= SHOOT_INTERVAL;
                ShootBullet();
            }
            if (Vector3.Distance(owner.transform.position, owner.playerCollider.bounds.center) > MAX_SHOOT_RANGE) {
                doShoot = false;
            }
        }
    }
    void SetDestination(Vector3 destination) {
        navMeshAgent.SetDestination(destination);
    }
    void ShootBullet() {
        PlayerInput.FireInputs input = new PlayerInput.FireInputs() {
            FirePressed = true,
            FireHeld = false,
            targetData = new TargetData2 {
                type = TargetData2.TargetType.objectLock,
                clickRay = new Ray(),
                screenPosition = Vector3.zero,
                highlightableTargetData = null,
                position = owner.lastSeenPlayerPosition
            }
        };
        gunHandler.ShootImmediately(input);
    }
    public override PlayerInput getInput() {
        Vector3 inputVector = doShoot ? Vector3.zero : navMeshAgent.desiredVelocity.normalized;
        bool reload = false;
        if (gunHandler.state == GunHandler.GunState.reloading) {
            gunHandler.ClipIn();
            gunHandler.StopReload();
        } else {
            reload = gunHandler.gunInstance.clip <= 0;
        }
        return new PlayerInput() {
            inputMode = InputMode.gun,
            MoveAxisForward = 0f,
            MoveAxisRight = 0f,
            CameraRotation = Quaternion.identity,
            JumpDown = false,
            jumpHeld = false,
            jumpReleased = false,
            CrouchDown = false,
            runDown = false,
            Fire = new PlayerInput.FireInputs(),
            reload = reload,
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
            doShootCountdown = SHOOT_TIMEOUT;
        }
    }
}