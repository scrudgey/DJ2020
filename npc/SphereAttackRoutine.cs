using UnityEngine;
using UnityEngine.AI;
public class SphereAttackRoutine : SphereControlState {
    enum State { none, approach, shoot, reload }
    State state;
    public SphereRobotSpeaker speaker;
    readonly float REPATH_INTERVAL = 0.1f;
    readonly float ATTACK_TIMEOUT = 3f;
    readonly float ROUTINE_TIMEOUT = 10f;
    readonly float SHOOT_TIMEOUT = 1f;
    readonly float SHOOT_INTERVAL = 0.1f;
    readonly float MAX_SHOOT_RANGE = 5f;
    private float newDestinationTimer;
    float timeSinceSawPlayer;
    float repathCountDown;
    float changeStateCountDown;
    float reloadCountDown;
    public GunHandler gunHandler;
    float doShootCountdown;
    float shootTimer;
    int pathIndex;
    private readonly float CORNER_ARRIVAL_DISTANCE = 0.1f;

    public SphereAttackRoutine(SphereRobotAI ai,
                                GunHandler gunHandler) : base(ai) {
        this.gunHandler = gunHandler;
        speaker = owner.GetComponent<SphereRobotSpeaker>();
        speaker.DoAttackSpeak();
    }
    public override void Enter() {
        changeStateCountDown = ROUTINE_TIMEOUT;
    }

    void ChangeState(State newState) {
        // Debug.Log("change state to " + newState);
        if (newState == State.shoot) {
            doShootCountdown = SHOOT_TIMEOUT;
        }
        state = newState;
    }
    public override void Update() {
        timeSinceSawPlayer += Time.deltaTime;
        repathCountDown -= Time.deltaTime;
        changeStateCountDown -= Time.deltaTime;
        if (changeStateCountDown <= 0) {
            owner.ChangeState(new SphereMoveRoutine(owner, owner.patrolZone));
        }
        if (repathCountDown <= 0) {
            repathCountDown += REPATH_INTERVAL;
            SetDestination(owner.lastSeenPlayerPosition);
        }
        if (state == State.shoot) {
            doShootCountdown -= Time.deltaTime;
            shootTimer += Time.deltaTime;
            if (shootTimer > SHOOT_INTERVAL) {
                shootTimer -= SHOOT_INTERVAL;
                ShootBullet();
            }
            if (doShootCountdown <= 0) {
                ChangeState(State.approach);
            }
            if (Vector3.Distance(owner.transform.position, owner.lastSeenPlayerPosition) > MAX_SHOOT_RANGE) {
                ChangeState(State.approach);
            }
        } else if (state == State.approach) {
            float distance = Vector3.Distance(owner.transform.position, owner.lastSeenPlayerPosition);
            if (distance <= MAX_SHOOT_RANGE && timeSinceSawPlayer < ATTACK_TIMEOUT) {
                ChangeState(State.shoot);
            }
        } else if (state == State.none) {
            ChangeState(State.approach);
        } else if (state == State.reload) {
            reloadCountDown -= Time.deltaTime;
            if (reloadCountDown <= 0)
                ChangeState(State.approach);
        }
    }
    void SetDestination(Vector3 destination) {
        NavMeshHit hit = new NavMeshHit();
        if (NavMesh.SamplePosition(destination, out hit, 10f, NavMesh.AllAreas)) {
            NavMesh.CalculatePath(owner.transform.position, hit.position, NavMesh.AllAreas, owner.navMeshPath);
            pathIndex = 1;
        } else {
            Debug.Log("could not find navmeshhit");
        }
    }
    void ShootBullet() {
        if (owner.lastSeenPlayerPosition == null)
            return;
        PlayerInput.FireInputs input = new PlayerInput.FireInputs() {
            FirePressed = true,
            FireHeld = false,
            targetData = new TargetData2 {
                type = TargetData2.TargetType.objectLock,
                screenPosition = Vector3.zero,
                highlightableTargetData = null,
                position = owner.lastSeenPlayerPosition
            }
        };
        gunHandler.ShootImmediately(input);
    }
    public override PlayerInput getInput() {
        Vector3 inputVector = Vector3.zero;

        if (state == State.approach) {
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
        }

        bool reload = false;
        if (gunHandler.state == GunHandler.GunState.reloading) {
            gunHandler.ClipIn();
            gunHandler.StopReload();
        } else {
            reload = gunHandler.gunInstance.clip <= 0;
            if (reload) {
                ChangeState(State.reload);
                reloadCountDown = 1f;
            }
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
            changeStateCountDown = ROUTINE_TIMEOUT;
            timeSinceSawPlayer = 0;
        }
    }

}

/**
 check time since we last saw before we shoot
 pursuit: a new routine? or part of attack?

 separate last seen time timeout from overall attack timeout:
 this lets robot go back to approach when it has started shooting at empty last seen position
 */