using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using KinematicCharacterController;
using UnityEngine;
public enum CharacterState {
    normal,
    wallPress,
    climbing,
    jumpPrep,
    superJump,
    landStun,
    dead,
    hitstun,
    keelOver,
    aim,
    popout,
    burgle
}
public enum ClimbingState {
    Anchoring,
    Climbing,
    DeAnchoring
}
public enum PopoutParity {
    left, right
}
public class CharacterController : MonoBehaviour, ICharacterController, IPlayerStateLoader, IBindable<CharacterController>, IInputReceiver, IHitstateSubscriber, IPoolable {
    public KinematicCharacterMotor Motor;
    public CharacterHurtable characterHurtable;
    public CharacterCamera OrbitCamera;
    public Transform targetPoint;
    public JumpIndicatorController jumpIndicatorController;
    public GunHandler gunHandler;
    public ItemHandler itemHandler;
    public Interactor interactor;
    public ManualHacker manualHacker;
    public Burglar burglar;
    public Footsteps footsteps;
    public AudioSource audioSource;
    public float defaultRadius = 0.10f;
    public Action<CharacterController> OnCharacterDead;
    public Action<CharacterController> OnValueChanged { get; set; }

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15;
    public float OrientationSharpness = 10;
    public float stepHeight = 0.2f;
    [Header("Crawling")]
    public float crawlSpeedFraction = 1f;
    public float crawlStickiness = 0.1f;
    public float crawlStepHeight = 0.02f;

    [Header("Running")]
    public float runSpeedFraction = 2f;

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 10f;
    public float AirAccelerationSpeed = 5f;
    public float Drag = 0.1f;

    [Header("Jumping")]
    public bool AllowJumpingWhenSliding = false;
    public bool AllowDoubleJump = false;
    public bool AllowWallJump = false;
    public float JumpSpeed = 10f;
    public float superJumpSpeed = 20f;
    public float JumpPreGroundingGraceTime = 0f;
    public float JumpPostGroundingGraceTime = 0f;
    private float jumpHeldTimer;
    public float jumpTimerThreshold = 0.5f;
    public AudioClip[] superJumpSounds;
    public AudioClip[] jumpPrepSounds;
    public AudioClip[] landingSounds;
    public AudioClip[] crouchingSounds;
    public AudioClip[] crawlSounds;
    public bool superJumpEnabled;
    public bool thirdGunSlotEnabled;

    [Header("Ladder Climbing")]
    public float ClimbingSpeed = 4f;
    public float AnchoringDuration = 0.25f;
    public LayerMask InteractionLayer;

    [Header("Misc")]
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public Vector3 direction;
    private Collider[] _probedColliders = new Collider[8];
    private Vector2 _moveAxis;
    private Vector3 _moveInputVector;
    private Vector3 _inputTorque;
    private Vector3 slewLookVector;
    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private bool _jumpedFromLadder = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private bool _doubleJumpConsumed = false;
    private bool _canWallJump = false;
    private Vector3 _wallJumpNormal;
    private Vector3 _internalVelocityAdd = Vector3.zero;
    private Vector3 snapToDirection = Vector2.zero;
    public bool isCrouching = false;
    public bool isRunning = false;
    public bool isProne = false;

    [Header("Wall press")]
    public float pressRadius = 0.1f;
    public float wallPressTimer = 0f;
    public float wallPressThreshold = 0.5f;
    public bool wallPressRatchet = false;
    public float wallPressHeight = 0.8f;
    public AudioClip[] wallPressSounds;
    public Vector3 wallNormal = Vector3.zero;
    public Vector2 lastWallInput = Vector2.zero;
    private CharacterState _state;
    [Header("Popout")]
    private Vector3 popOutPosition;
    private Vector3 prePopPosition;
    private bool atRightEdge;
    private bool atLeftEdge;
    private Vector3 popLeftPosition;
    private Vector3 popRightPosition;
    private PopoutParity popoutParity;
    public CharacterState state {
        get { return _state; }
    }
    public HitState hitState { get; set; }

    // Ladder vars
    private float _ladderUpDownInput;
    private Ladder _activeLadder { get; set; }
    private ClimbingState _internalClimbingState;
    private ClimbingState _climbingState {
        get {
            return _internalClimbingState;
        }
        set {
            _internalClimbingState = value;
            _anchoringTimer = 0f;
            _anchoringStartPosition = Motor.TransientPosition;
            _anchoringStartRotation = Motor.TransientRotation;
        }
    }
    private Vector3 _ladderTargetPosition;
    private Quaternion _ladderTargetRotation;
    private float _onLadderSegmentState = 0;
    private float _anchoringTimer = 0f;
    private Vector3 _anchoringStartPosition = Vector3.zero;
    private Quaternion _anchoringStartRotation = Quaternion.identity;
    private Quaternion _rotationBeforeClimbing = Quaternion.identity;

    private float landStunTimer;
    private PlayerInput _lastInput = PlayerInput.none;
    private CursorData lastTargetDataInput;
    private Vector3 lookAtDirection;
    private float inputDirectionHeldTimer;
    private float crouchMovementInputTimer;
    private float crouchMovementSoundTimer;
    private bool inputCrouchDown;
    private Vector3 deadMoveVelocity;
    private float deadTimer;
    private float hitstunTimer;
    private float gunHitStunTimer;
    private Quaternion aimCameraRotation;
    private Vector3 recoil;
    private float aimSwayTimer;
    private float aimSwayFrequencyConstant = 0.5f;
    private float aimSwayMagnitude = 0.01f;
    Transform cameraFollowTransform;
    RaycastHit[] rayCastHits;

    public void TransitionToState(CharacterState newState) {
        CharacterState tmpInitialState = state;
        OnStateExit(tmpInitialState, newState);
        _state = newState;
        OnStateEnter(newState, tmpInitialState);
        OnValueChanged?.Invoke(this);
    }
    private void OnStateEnter(CharacterState state, CharacterState fromState) {
        // Debug.Log($"entering state {state} from {fromState}");
        switch (state) {
            default:
                break;
            case CharacterState.burgle:
                // GameManager.I.StartBurglar();
                GameManager.I.TransitionToInputMode(InputMode.burglar);
                break;
            case CharacterState.wallPress:
                GameManager.I.TransitionToInputMode(InputMode.wallpressAim);
                break;
            case CharacterState.popout:
                GameManager.I.TransitionToInputMode(InputMode.wallpressAim);
                break;
            case CharacterState.aim:
                aimCameraRotation = Quaternion.FromToRotation(Vector3.forward, Motor.CharacterForward);
                lookAtDirection = Motor.CharacterForward;
                GameManager.I.TransitionToInputMode(InputMode.aim);
                break;
            case CharacterState.jumpPrep:
                Toolbox.RandomizeOneShot(audioSource, jumpPrepSounds);
                break;
            case CharacterState.superJump:
                Toolbox.RandomizeOneShot(audioSource, superJumpSounds);
                Time.timeScale = 0.75f;
                break;
            case CharacterState.landStun:
                landStunTimer = 0.5f;
                Toolbox.RandomizeOneShot(audioSource, landingSounds);
                isCrouching = true;
                PoolManager.I.GetPool("prefabs/fx/landImpactFx").GetObject(transform.position);
                Ray ray = new Ray(transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down);
                RaycastHit[] hits = Physics.RaycastAll(ray, 1f, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive));
                foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
                    if (hit.collider.transform.IsChildOf(transform.root))
                        continue;
                    GameObject decalObject = PoolManager.I.CreateDecal(hit, PoolManager.DecalType.explosiveScar);
                    decalObject.transform.SetParent(hit.collider.transform, true);
                }
                break;
            case CharacterState.climbing:
                _rotationBeforeClimbing = Motor.TransientRotation;

                Motor.SetMovementCollisionsSolvingActivation(false);
                Motor.SetGroundSolvingActivation(false);
                _climbingState = ClimbingState.Anchoring;

                // Store the target position and rotation to snap to
                _ladderTargetPosition = _activeLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                _ladderTargetRotation = _activeLadder.transform.rotation;
                Toolbox.RandomizeOneShot(audioSource, _activeLadder.sounds);
                break;
            case CharacterState.dead:
                break;

        }
    }

    public void OnStateExit(CharacterState state, CharacterState toState) {
        switch (state) {
            default:
                break;
            case CharacterState.wallPress:
            case CharacterState.popout:
            case CharacterState.aim:
                if (toState != CharacterState.popout && toState != CharacterState.aim && toState != CharacterState.wallPress)
                    GameManager.I.TransitionToInputMode(InputMode.gun);
                break;
            case CharacterState.landStun:
                isCrouching = true;
                break;
            case CharacterState.superJump:
                Time.timeScale = 1f;
                _jumpedFromLadder = false;
                break;
            case CharacterState.climbing:
                Toolbox.RandomizeOneShot(audioSource, _activeLadder.sounds);
                Motor.SetMovementCollisionsSolvingActivation(true);
                Motor.SetGroundSolvingActivation(true);
                break;
        }
    }
    private void Start() {
        rayCastHits = new RaycastHit[32];
        PoolManager.I.RegisterPool("prefabs/fx/landImpactFx", poolSize: 2);
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        gunHandler.characterCamera = OrbitCamera;
        gunHandler.OnShoot += HandleOnShoot;
        // Assign to motor
        Motor.CharacterController = this;
        characterHurtable.OnHitStateChanged += HandleHurtableChanged;
        characterHurtable.OnTakeDamage += HandleTakeDamage;
        cameraFollowTransform = transform.Find("cameraFollowPoint");
    }

    void OnDestroy() {
        characterHurtable.OnHitStateChanged -= HandleHurtableChanged;
        characterHurtable.OnTakeDamage -= HandleTakeDamage;
        gunHandler.OnShoot -= HandleOnShoot;
    }
    void HandleOnShoot(GunHandler target) {
        float recoilMagnitudeY = target.gunInstance.template.recoil.GetRandomInsideBound();
        float recoilMagnitudeX = target.gunInstance.template.recoil.GetRandomInsideBound() * UnityEngine.Random.Range(-0.5f, 0.5f);
        recoil = new Vector3(recoilMagnitudeX, recoilMagnitudeY, 0f);
    }
    private void HandleHurtableChanged(Destructible hurtable) {
        ((IHitstateSubscriber)this).TransitionToHitState(hurtable.hitState);
        if (hurtable.lastDamage != null)
            deadMoveVelocity = hurtable.lastDamage.direction;
    }
    private void HandleTakeDamage(Damageable damageable, Damage damage) {
        hitstunTimer = 0.15f;
        gunHitStunTimer = 0.5f;
    }
    public void OnHitStateEnter(HitState state, HitState fromState) {
        switch (state) {
            default:
                break;
            case HitState.hitstun:
                TransitionToState(CharacterState.hitstun);
                break;
            case HitState.dead:
                TransitionToState(CharacterState.dead);

                break;
        }
        OnValueChanged?.Invoke(this);
    }
    public void OnHitStateExit(HitState state, HitState toState) {
        switch (state) {
            default:
                break;
            case HitState.hitstun:
                TransitionToState(CharacterState.normal);
                break;
        }
        OnValueChanged?.Invoke(this);
    }

    public void ResetInput() {
        SetInputs(PlayerInput.none);
    }
    /// <summary>
    /// This is called every frame by MyPlayer in order to tell the character what its inputs are
    /// </summary>
    public void SetInputs(PlayerInput input) {
        // Handle ladder transitions
        _ladderUpDownInput = input.MoveAxisForward;
        if (input.actionButtonPressed) {
            _probedColliders = new Collider[8];
            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, InteractionLayer, QueryTriggerInteraction.Collide) > 0) {
                foreach (Collider collider in _probedColliders) {
                    // Handle ladders
                    if (collider == null || collider.gameObject == null)
                        continue;
                    Ladder ladder = collider.gameObject.GetComponent<Ladder>();
                    if (ladder) {
                        // Transition to ladder climbing state
                        if (state == CharacterState.normal) {
                            _activeLadder = ladder;
                            TransitionToState(CharacterState.climbing);
                        }
                        // Transition back to default movement state
                        else if (state == CharacterState.climbing) {
                            _climbingState = ClimbingState.DeAnchoring;
                            _ladderTargetPosition = Motor.TransientPosition;
                            _ladderTargetRotation = _rotationBeforeClimbing;
                        }
                        break;
                    }
                }
            }
        }

        // Clamp input
        // TODO: this is weird
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(input.MoveAxisRight, 0f, input.MoveAxisForward), 1f);
        if (input.moveDirection != Vector3.zero) {
            moveInputVector = Vector3.ClampMagnitude(input.moveDirection, 1f);
        } else if (moveInputVector.y != 0 && moveInputVector.x != 0) {
            moveInputVector = CharacterCamera.rotationOffset * moveInputVector;
        }

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(input.CameraRotation * Vector3.forward, Vector3.up).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f) {
            cameraPlanarDirection = Vector3.ProjectOnPlane(input.CameraRotation * Vector3.up, Vector3.up).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Vector3.up);

        // Move and look inputs
        _moveInputVector = cameraPlanarRotation * moveInputVector;
        if (input.MoveAxisRight != 0 && input.MoveAxisForward != 0) {
            _moveInputVector = Quaternion.Inverse(CharacterCamera.rotationOffset) * _moveInputVector;
        }
        _moveAxis = new Vector2(input.MoveAxisRight, input.MoveAxisForward);


        // Run input
        Motor.MaxStepHeight = stepHeight;
        if (input.runDown) {
            isRunning = true;
        } else {
            isRunning = false;

            // Crouching input
            if (input.CrouchDown || input.jumpHeld || state == CharacterState.landStun || state == CharacterState.jumpPrep) {
                SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
                if (!isCrouching) {
                    isCrouching = true;
                    if (input.CrouchDown)
                        Toolbox.RandomizeOneShot(audioSource, crouchingSounds);
                }
                Motor.MaxStepHeight = crawlStepHeight;
            }
            if (!isProne && isMoving() && isCrouching) {
                isProne = true;
            } else if (isProne && !isMoving() && input.unCrawl) {
                isProne = false;
            }
            if (isProne && !isCrouching) {
                isProne = false;
            }
        }
        if (input.Fire.FirePressed || input.Fire.FireHeld) {
            isProne = false;
        }

        if (moveInputVector == Vector3.zero && input.jumpHeld && Motor.GroundingStatus.IsStableOnGround) {
            jumpHeldTimer += Time.deltaTime;
            if (superJumpEnabled && jumpHeldTimer > jumpTimerThreshold && state != CharacterState.jumpPrep) {
                TransitionToState(CharacterState.jumpPrep);
            }
        } else {
            jumpHeldTimer = 0;
        }

        inputCrouchDown = input.CrouchDown;
        CursorData cursorData = input.Fire.cursorData;
        if (input.selectgun == 3 && !thirdGunSlotEnabled)
            input.selectgun = 0;
        switch (state) {
            case CharacterState.hitstun:
            case CharacterState.dead:
                break;
            case CharacterState.jumpPrep:
                // TODO: normalize this player state
                jumpIndicatorController.superJumpSpeed = superJumpSpeed;
                jumpIndicatorController.gravity = Gravity;
                jumpIndicatorController.SetInputs(input);
                SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
                if (input.jumpReleased) {
                    _timeSinceJumpRequested = 0f;
                    _jumpRequested = true;
                }
                break;
            case CharacterState.aim:
                isRunning = false;
                isProne = false;
                if (input.CrouchDown) {
                    SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
                    if (!isCrouching) {
                        isCrouching = true;
                        Toolbox.RandomizeOneShot(audioSource, crouchingSounds);
                    }
                    Motor.MaxStepHeight = crawlStepHeight;
                }

                // Fire

                gunHandler.ProcessGunSwitch(input);
                gunHandler.SetInputs(input);

                _inputTorque = input.mouseDelta;

                aimSwayTimer += Time.deltaTime;

                float aimSwayX = Mathf.Cos(aimSwayTimer * aimSwayFrequencyConstant);
                float aimSwayY = Mathf.Cos(aimSwayTimer * aimSwayFrequencyConstant * 2f);
                Vector3 aimSway = new Vector3(aimSwayX, aimSwayY, 0f) * aimSwayMagnitude;
                _inputTorque += aimSway;

                if (input.Fire.AimPressed) {
                    TransitionToState(CharacterState.normal);
                }
                break;
            case CharacterState.normal:
                wallPressRatchet = false;
                if (hitstunTimer <= 0) {
                    // Items
                    itemHandler.SetInputs(input);

                    // Cyberdeck
                    ManualHackInput manualHackInput = new ManualHackInput {
                        playerInput = input,
                        activeItem = itemHandler.activeItem
                    };
                    manualHacker?.SetInputs(manualHackInput);
                    burglar?.SetInputs(manualHackInput);
                    if (interactor != null) {
                        interactor.SetInputs(input);
                    }
                }

                if (gunHitStunTimer > 0) {
                    input.Fire = PlayerInput.FireInputs.none;
                }

                // Fire
                gunHandler.ProcessGunSwitch(input);
                gunHandler.SetInputs(input);

                // Turn to face cursor or movement direction
                // TODO: exclude when crawling
                if (input.lookAtPosition != Vector3.zero) {
                    lookAtDirection = input.lookAtPosition - transform.position;
                }
                if (input.lookAtDirection != Vector3.zero) {
                    lookAtDirection = input.lookAtDirection;
                }
                if (isCrouching && !isProne && crouchMovementInputTimer < 0.3f) {
                    snapToDirection = _moveInputVector;
                }

                slewLookVector = Vector3.zero;
                if (input.Fire.AimPressed || input.Fire.FireHeld || input.Fire.FirePressed) {
                    Vector3 directionToCursor = cursorData.worldPosition - transform.position;
                    directionToCursor.y = 0;
                    snapToDirection = directionToCursor;
                } else if (input.orientTowardDirection != Vector3.zero) {
                    slewLookVector = input.orientTowardDirection;
                } else if (input.orientTowardPoint != Vector3.zero) {
                    slewLookVector = input.orientTowardPoint - transform.position;
                } else {
                    slewLookVector = _moveInputVector;
                }
                slewLookVector.y = 0;

                // if (input.Fire.AimPressed) {
                //     snapToDirection = lookAtDirection;
                // }
                if (input.snapToLook) {
                    if (input.lookAtPosition != Vector3.zero) {
                        snapToDirection = input.lookAtPosition - transform.position;
                    }
                    if (input.lookAtDirection != Vector3.zero) {
                        snapToDirection = input.lookAtDirection;
                    }
                }

                // Jumping input
                if (input.jumpReleased) {
                    _timeSinceJumpRequested = 0f;
                    _jumpRequested = true;
                }
                if (isCrouching && _moveInputVector != Vector3.zero) {
                    crouchMovementInputTimer += Time.deltaTime;
                } else {
                    crouchMovementInputTimer = 0f;
                    crouchMovementInputTimer = 1f;
                }

                if (input.Fire.AimPressed) {
                    TransitionToState(CharacterState.aim);
                }
                break;
            case CharacterState.popout:
                gunHandler.ProcessGunSwitch(input);
                gunHandler.SetInputs(input);
                if (input.lookAtDirection != Vector3.zero) {
                    lookAtDirection = input.lookAtDirection;
                }
                if (input.Fire.AimPressed) {
                    TransitionToState(CharacterState.wallPress);
                }
                if (Math.Abs(input.MoveAxis().y) > 0.1) {
                    TransitionToState(CharacterState.normal);
                }
                break;
            case CharacterState.wallPress:
                if (_moveAxis != Vector2.zero) {
                    lastWallInput = _moveAxis;
                } else {
                    wallPressRatchet = true;
                }
                if (!wallPressRatchet) {
                    _moveInputVector = Vector3.zero;
                    _moveAxis = Vector2.zero;
                }

                // transition to popout
                if (input.Fire.AimPressed || input.Fire.FirePressed) {
                    if (atLeftEdge) {
                        popOutPosition = popLeftPosition;
                        prePopPosition = transform.position;
                        popoutParity = PopoutParity.left;
                        TransitionToState(CharacterState.popout);
                    } else if (atRightEdge) {
                        popOutPosition = popRightPosition;
                        prePopPosition = transform.position;
                        popoutParity = PopoutParity.right;
                        TransitionToState(CharacterState.popout);
                    }
                }

                // allow gun switch
                input.Fire.FireHeld = false;
                input.Fire.FirePressed = false;
                gunHandler.ProcessGunSwitch(input);
                gunHandler.SetInputs(input);

                break;
            case CharacterState.climbing:
                if (input.jumpReleased) {
                    _timeSinceJumpRequested = 0f;
                    _jumpRequested = true;
                }
                break;
            case CharacterState.superJump:
                if (Motor.Velocity.y < 0) {
                    SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
                } else {
                    SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
                }
                break;
            case CharacterState.landStun:
                SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
                break;
        }

        if (Vector2.Distance(InputThreshold(input.MoveAxis()), InputThreshold(_lastInput.MoveAxis())) > 0.1f) {
            inputDirectionHeldTimer = 0f;
        } else {
            inputDirectionHeldTimer += Time.deltaTime;
        }
        _lastInput = input;
        lastTargetDataInput = cursorData;
    }

    Vector2 InputThreshold(Vector2 input) {
        float X = input.x switch {
            float x when x > 0.5 => 1,
            float x when x < -0.5 => -1,
            _ => 0
        };
        float Y = input.y switch {
            float y when y > 0.5 => 1,
            float y when y < 0.5 => -1,
            _ => 0
        };
        Vector2 output = new Vector2(X, Y);
        return output;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime) {
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
        if (IsMovementSticking()) {
            return;
        }
        switch (state) {
            case CharacterState.hitstun:
            case CharacterState.dead:
                slewLookVector = -1f * Motor.Velocity;
                lookAtDirection = -1f * Motor.Velocity;
                break;
            case CharacterState.aim:
                if (snapToDirection != Vector3.zero) {
                    Vector3 target = snapToDirection;
                    target.y = 0;
                    currentRotation = Quaternion.LookRotation(target, Vector3.up);
                    aimCameraRotation = currentRotation;
                } else {
                    // apply input torque from mouse delta
                    Vector3 targetRight = aimCameraRotation * Vector3.right;
                    Vector3 targetUp = Vector3.up;
                    Quaternion pitch = Quaternion.AngleAxis(-1f * (_inputTorque.y + recoil.y), targetRight);
                    Quaternion yaw = Quaternion.AngleAxis(_inputTorque.x + recoil.x, targetUp);
                    aimCameraRotation = pitch * yaw * aimCameraRotation;

                    // reorient camera to be level with horizontal
                    Vector3 aimCameraRight = aimCameraRotation * Vector3.right;
                    Vector3 projectedAimCamerRight = Vector3.ProjectOnPlane(aimCameraRight, Vector3.up);
                    Quaternion roll = Quaternion.FromToRotation(aimCameraRight, projectedAimCamerRight);
                    aimCameraRotation = roll * aimCameraRotation;

                    // lerp character orientation toward the aim point
                    Vector3 projectedCameraForward = Vector3.ProjectOnPlane(aimCameraRotation * Vector3.forward, Vector3.up);
                    Quaternion levelRotation = Quaternion.LookRotation(projectedCameraForward, Vector3.up);
                    currentRotation = Quaternion.Lerp(currentRotation, levelRotation, 0.05f);

                    // clamp pitch [0, 45] , [365, 315]
                    Vector3 currentRotationEulerAngles = aimCameraRotation.eulerAngles;
                    if (currentRotationEulerAngles.x > 180) {
                        currentRotationEulerAngles.x = Mathf.Max(currentRotationEulerAngles.x, 315f);
                    } else {
                        currentRotationEulerAngles.x = Mathf.Min(currentRotationEulerAngles.x, 45f);
                    }
                    aimCameraRotation = Quaternion.Euler(currentRotationEulerAngles);

                    lookAtDirection = Vector3.Lerp(lookAtDirection, aimCameraRotation * Vector3.forward, 0.05f);
                }
                break;
            case CharacterState.normal:
                if (snapToDirection != Vector3.zero) {
                    Vector3 target = snapToDirection;
                    target.y = 0;
                    currentRotation = Quaternion.LookRotation(target, Vector3.up);
                } else if (wallPressTimer > 0 && wallNormal != Vector3.zero) { // wall pressing
                                                                               // Smoothly interpolate from current to target look direction
                    Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, wallNormal, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                    // Set the current rotation (which will be used by the KinematicCharacterMotor)
                    // currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Vector3.up);
                } else if (slewLookVector != Vector3.zero && OrientationSharpness > 0f) {
                    // Smoothly interpolate from current to target look direction
                    float sharpness = OrientationSharpness;
                    if (isCrouching) sharpness *= 0.15f;
                    Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, slewLookVector, 1 - Mathf.Exp(-sharpness * deltaTime)).normalized;
                    // Set the current rotation (which will be used by the KinematicCharacterMotor)
                    currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Vector3.up);
                }
                break;
            case CharacterState.popout:
                currentRotation = Quaternion.LookRotation(-1f * wallNormal, Vector3.up);
                break;
            case CharacterState.wallPress:
                currentRotation = Quaternion.LookRotation(wallNormal, Vector3.up);
                break;
            case CharacterState.jumpPrep:
                Vector3 indicatorDirection = jumpIndicatorController.indicator.transform.position - transform.position;
                indicatorDirection.y = 0;
                indicatorDirection = indicatorDirection.normalized;
                Vector3 smoothedLookDirection = Vector3.Slerp(Motor.CharacterForward, indicatorDirection, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                currentRotation = Quaternion.LookRotation(smoothedLookDirection, Vector3.up);
                break;
            case CharacterState.climbing:
                switch (_climbingState) {
                    case ClimbingState.Climbing:
                        currentRotation = _activeLadder.transform.rotation;
                        break;
                    case ClimbingState.Anchoring:
                    case ClimbingState.DeAnchoring:
                        currentRotation = Quaternion.Slerp(_anchoringStartRotation, _ladderTargetRotation, (_anchoringTimer / AnchoringDuration));
                        break;
                }
                break;
        }
        slewLookVector = Vector3.zero;
        snapToDirection = Vector3.zero;
    }
    bool ColliderRay(Vector3 offset) {
        Vector3 start = transform.position + offset;
        Vector3 dir = -1f * wallNormal;
        float length = 0.4f;
        Debug.DrawRay(start, length * dir, new Color(162, 142, 149));
        int numberHit = Physics.RaycastNonAlloc(start, dir, rayCastHits, length, LayerUtil.GetLayerMask(Layer.def, Layer.obj), QueryTriggerInteraction.Ignore);
        // for (int i = 0; i < numberHit; i++) {
        //     Debug.Log(rayCastHits[i].collider);
        // }
        return numberHit > 0;
    }
    bool DetectWallPress() {
        if (state == CharacterState.aim)
            return false;
        if (wallNormal == Vector3.zero)
            return false;
        if (prePopPosition != Vector3.zero)
            return true;
        return ColliderRay(new Vector3(0f, wallPressHeight, 0f));
    }
    bool AtRightEdge() {
        if (wallNormal == Vector3.zero) {
            popRightPosition = transform.position;
            return false;
        }
        Vector3 offset = new Vector3(0f, wallPressHeight, 0f) - 0.4f * Motor.CharacterRight;
        popRightPosition = transform.position - 0.4f * Motor.CharacterRight;
        return !ColliderRay(offset);
    }
    bool AtLeftEdge() {
        if (wallNormal == Vector3.zero) {
            popLeftPosition = transform.position;
            return false;
        }
        Vector3 offset = new Vector3(0f, wallPressHeight, 0f) + 0.4f * Motor.CharacterRight;
        popLeftPosition = transform.position + 0.4f * Motor.CharacterRight;
        return !ColliderRay(offset);
    }

    Ladder GetOverlappingLadder() {
        _probedColliders = new Collider[8];
        if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, InteractionLayer, QueryTriggerInteraction.Collide) > 0) {
            foreach (Collider collider in _probedColliders) {
                if (collider == null || collider.gameObject == null)
                    continue;
                Ladder ladder = collider.gameObject.GetComponent<Ladder>();
                if (ladder) {
                    return ladder;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        bool pressingOnWall = _lastInput.preventWallPress ? false : DetectWallPress();
        Vector3 targetMovementVelocity = Vector3.zero;
        switch (state) {
            case CharacterState.burgle:
                currentVelocity = currentVelocity * 0.9f;
                break;
            case CharacterState.hitstun:
                currentVelocity = currentVelocity * 0.9f;
                break;
            case CharacterState.dead:
                deadTimer += Time.deltaTime;
                if (deadTimer <= 1.5f) {
                    currentVelocity = (float)PennerDoubleAnimation.QuintEaseOut(deadTimer, 4, -4, 1.25f) * deadMoveVelocity;
                }
                // if (Motor.GroundingStatus.IsStableOnGround) {
                //     currentVelocity += Gravity * deltaTime;
                // }
                if (deadTimer >= 1.1f) {
                    TransitionToState(CharacterState.keelOver);
                }
                // if (damag)
                break;
            case CharacterState.keelOver:
                deadTimer += Time.deltaTime;
                currentVelocity = Vector3.zero;
                // if (Motor.GroundingStatus.IsStableOnGround) {
                //     currentVelocity += Gravity * deltaTime;
                // }
                if (deadTimer >= 1.8f) {
                    OnCharacterDead?.Invoke(this);
                    CreateCorpse();
                    PoolManager.I.RecallObject(transform.root.gameObject);
                }
                break;
            case CharacterState.jumpPrep:
                _jumpedThisFrame = false;
                _timeSinceJumpRequested += deltaTime;
                if (_jumpRequested) {
                    // See if we actually are allowed to jump
                    if (!_jumpConsumed && (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)) {
                        // Calculate jump direction before ungrounding
                        // Vector3 jumpDirection = (Vector3.up + 0.2f * Motor.CharacterForward) * superJumpSpeed;
                        Motor.ForceUnground(0.1f);

                        // Add to the return velocity and reset jump state
                        Vector3 jumpVelocity = Toolbox.SuperJumpVelocity(jumpIndicatorController.indicator.localPosition, superJumpSpeed, Gravity.y);
                        jumpVelocity = jumpIndicatorController.transform.localToWorldMatrix * jumpVelocity;
                        // jumpVelocity -= Vector3.Project(currentVelocity, Vector3.up);
                        currentVelocity = jumpVelocity;
                        _jumpRequested = false;
                        _jumpConsumed = true;
                        _jumpedThisFrame = true;

                        _jumpedFromLadder = GetOverlappingLadder() != null;
                        TransitionToState(CharacterState.superJump);
                    }
                } else {

                    // Smooth movement Velocity
                    currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                }
                // Reset wall jump
                _canWallJump = false;

                break;
            case CharacterState.superJump:
                currentVelocity += Gravity * deltaTime;
                // currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                if (Motor.GroundingStatus.IsStableOnGround && !_jumpedThisFrame) {
                    if (Motor.Velocity.y < -0.3f) {
                        TransitionToState(CharacterState.landStun);
                    } else {
                        TransitionToState(CharacterState.normal);
                    }
                }
                _jumpedThisFrame = false;
                Ladder ladder = GetOverlappingLadder();
                if (ladder != null) {
                    if (!_jumpedFromLadder) {
                        _activeLadder = ladder;
                        TransitionToState(CharacterState.climbing);
                    }
                } else {
                    _jumpedFromLadder = false;
                }
                break;
            case CharacterState.popout:
                currentVelocity = Vector3.zero;
                break;
            case CharacterState.wallPress:
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                atLeftEdge = AtLeftEdge();
                atRightEdge = AtRightEdge();
                if (atRightEdge) {
                    _moveAxis.x = Mathf.Min(0, _moveAxis.x);
                }
                if (atLeftEdge) {
                    _moveAxis.x = Mathf.Max(0, _moveAxis.x);
                }

                // wall style input is relative to the wall normal
                targetMovementVelocity = (Vector3.Cross(wallNormal, Motor.GroundingStatus.GroundNormal) * _moveAxis.x - wallNormal * _moveAxis.y) * MaxStableMoveSpeed * 0.5f;

                // transition from wall press 
                if (!pressingOnWall) {
                    TransitionToState(CharacterState.normal);
                }

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));

                // Gravity
                // currentVelocity += Gravity * deltaTime;
                break;
            case CharacterState.landStun:
                if (Motor.GroundingStatus.IsStableOnGround) {
                    SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                    currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, 0.2f);
                } else {
                    if (_moveInputVector.sqrMagnitude > 0f) {
                        targetMovementVelocity = Vector3.zero;
                        if (Motor.GroundingStatus.FoundAnyGround) {
                            Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                            targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                        }
                        Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                        currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                    }
                    currentVelocity += Gravity * deltaTime;
                    currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                }
                landStunTimer -= deltaTime;
                if (landStunTimer < 0) {
                    TransitionToState(CharacterState.normal);
                }
                break;
            case CharacterState.aim:
            case CharacterState.normal:
                if (Motor.GroundingStatus.IsStableOnGround) {
                    // Reorient velocity on slope
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                    // I am pressing on the wall, but not yet in wall press mode
                    if (pressingOnWall && Vector3.Dot(_moveInputVector, wallNormal) < -0.9 && Vector3.Dot(_moveInputVector, wallNormal) > -1.1) {
                        if (wallPressTimer == 0) {
                            Toolbox.RandomizeOneShot(audioSource, wallPressSounds);
                        }
                        wallPressTimer += Time.deltaTime;
                        prePopPosition = Vector3.zero;


                        // transition to wallpress mode
                        if (wallPressTimer > wallPressThreshold) {
                            TransitionToState(CharacterState.wallPress);
                            wallPressTimer = 0;
                        }
                    }
                    // I have let off input to press on wall. reset the state 
                    else if (_moveInputVector == Vector3.zero) {
                        wallPressTimer = 0;
                        wallNormal = Vector3.zero;
                    }
                    // I am not pressing on the wall, cool down the timer.
                    else if (wallPressTimer > 0) {
                        wallPressTimer -= Time.deltaTime;
                    }

                    // Calculate target velocity
                    Vector3 inputRight = Vector3.Cross(_moveInputVector, Vector3.up);
                    Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                    targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                    if (state == CharacterState.aim) {
                        if (isCrouching) {
                            targetMovementVelocity = Vector3.zero;
                        } else {
                            targetMovementVelocity /= 2f;
                        }
                    } else if (isRunning) {
                        targetMovementVelocity *= runSpeedFraction;
                    } else if (isCrouching) {// crawling
                        if (inputDirectionHeldTimer > crawlStickiness && crouchMovementInputTimer > 0.3f) {
                            if (targetMovementVelocity != Vector3.zero & !pressingOnWall) {
                                targetMovementVelocity = direction;
                                // play sound
                                crouchMovementSoundTimer += Time.deltaTime;
                                if (crouchMovementSoundTimer >= 1f) {
                                    Toolbox.RandomizeOneShot(audioSource, crawlSounds);
                                    crouchMovementSoundTimer -= 1f;
                                }
                            }
                            Vector3 initialMovementVelocity = targetMovementVelocity;
                            targetMovementVelocity *= crawlSpeedFraction * Toolbox.SquareWave(crouchMovementInputTimer, dutycycle: 0.75f);
                            targetMovementVelocity += 0.5f * crawlSpeedFraction * initialMovementVelocity;


                        } else {
                            targetMovementVelocity = Vector3.zero;
                        }
                    }
                    if (hitstunTimer > 0f) {
                        targetMovementVelocity = Vector3.zero;
                    }
                    // Smooth movement Velocity
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                } else {
                    // Add move input
                    if (_moveInputVector.sqrMagnitude > 0f) {
                        targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                        // Prevent climbing on un-stable slopes with air movement
                        if (Motor.GroundingStatus.FoundAnyGround) {
                            Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                            targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                        }

                        Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                        currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                    }

                    // Gravity
                    currentVelocity += Gravity * deltaTime;

                    // Drag
                    currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                }

                // Handle jumping
                _jumpedThisFrame = false;
                _timeSinceJumpRequested += deltaTime;
                if (_jumpRequested) {
                    // Handle double jump
                    if (AllowDoubleJump) {
                        if (_jumpConsumed && !_doubleJumpConsumed && (AllowJumpingWhenSliding ? !Motor.GroundingStatus.FoundAnyGround : !Motor.GroundingStatus.IsStableOnGround)) {
                            Motor.ForceUnground(0.1f);

                            // Add to the return velocity and reset jump state
                            // currentVelocity += (Motor.CharacterUp * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                            currentVelocity += (Vector3.up * JumpSpeed) - Vector3.Project(currentVelocity, Vector3.up);
                            _jumpRequested = false;
                            _doubleJumpConsumed = true;
                            _jumpedThisFrame = true;
                        }
                    }

                    // See if we actually are allowed to jump
                    if (_canWallJump ||
                        (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))) {
                        // Calculate jump direction before ungrounding
                        Vector3 jumpDirection = Vector3.up;
                        if (_canWallJump) {
                            jumpDirection = _wallJumpNormal;
                        } else if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround) {
                            jumpDirection = Motor.GroundingStatus.GroundNormal;
                        }

                        // Makes the character skip ground probing/snapping on its next update. 
                        // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                        Motor.ForceUnground(0.1f);

                        // Add to the return velocity and reset jump state
                        currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Vector3.up);
                        _jumpRequested = false;
                        _jumpConsumed = true;
                        _jumpedThisFrame = true;
                    }
                }

                // Reset wall jump
                _canWallJump = false;

                // Take into account additive velocity
                if (_internalVelocityAdd.sqrMagnitude > 0f) {
                    currentVelocity += _internalVelocityAdd;
                    _internalVelocityAdd = Vector3.zero;
                }

                if (!isCrouching && Motor.GroundingStatus.IsStableOnGround)
                    footsteps.UpdateWithVelocity(currentVelocity, isRunning);

                break;
            case CharacterState.climbing:
                currentVelocity = Vector3.zero;

                switch (_climbingState) {
                    case ClimbingState.Climbing:
                        currentVelocity = (_ladderUpDownInput * _activeLadder.transform.up).normalized * ClimbingSpeed;
                        footsteps.SetFootstepSounds(_activeLadder.surfaceType);
                        footsteps.UpdateWithVelocity(currentVelocity, false);
                        break;
                    case ClimbingState.Anchoring:
                    case ClimbingState.DeAnchoring:
                        Vector3 tmpPosition = Vector3.Lerp(_anchoringStartPosition, _ladderTargetPosition, (_anchoringTimer / AnchoringDuration));
                        currentVelocity = Motor.GetVelocityForMovePosition(Motor.TransientPosition, tmpPosition, deltaTime);
                        break;
                }

                if (_jumpRequested && !_jumpConsumed) {
                    Vector3 jumpDirection = Vector3.up;
                    // jumpDirection = Motor.GroundingStatus.GroundNormal;

                    Motor.ForceUnground(0.1f);
                    currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Vector3.up);
                    _jumpRequested = false;
                    _jumpConsumed = true;
                    _jumpedThisFrame = true;
                    TransitionToState(CharacterState.normal);
                }
                break;
        }
    }

    void CheckUncrouch() {
        if (isCrouching && !inputCrouchDown && state != CharacterState.landStun && state != CharacterState.jumpPrep) {
            // Do an overlap test with the character's standing height to see if there are any obstructions
            SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
            _probedColliders = new Collider[8];
            if (Motor.CharacterCollisionsOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders) > 0) {
                // If obstructions, just stick to crouching dimensions
                SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
            } else {
                // If no obstructions, uncrouch
                SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
                isCrouching = false;
            }
        }
    }
    void SetCapsuleDimensions(float radius, float height, float yOffset) {
        Motor.SetCapsuleDimensions(radius, height, yOffset);
        if (targetPoint != null) {
            if (height > 1f) {
                targetPoint.localPosition = new Vector3(0f, 1f, 0f);
            } else {
                targetPoint.localPosition = new Vector3(0f, 0.5f, 0f);
            }
        }
    }
    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime) {
        switch (state) {
            case CharacterState.landStun:
                break;
            case CharacterState.dead:
                break;
            case CharacterState.popout:
                Vector3 newPosition = Vector3.Lerp(transform.position, popOutPosition, 0.5f);
                Motor.SetPosition(newPosition);
                direction = Motor.CharacterForward;
                break;
            case CharacterState.aim:
            case CharacterState.normal:
                // Handle jump-related values
                // Handle jumping pre-ground grace period
                if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime) {
                    _jumpRequested = false;
                }

                if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) {
                    // If we're on a ground surface, reset jumping values
                    if (!_jumpedThisFrame) {
                        _doubleJumpConsumed = false;
                        _jumpConsumed = false;
                    }
                    _timeSinceLastAbleToJump = 0f;
                } else {
                    // Keep track of time since we were last able to jump (for grace period)
                    _timeSinceLastAbleToJump += deltaTime;
                }

                // Handle uncrouching
                CheckUncrouch();
                direction = Motor.CharacterForward;
                break;
            case CharacterState.wallPress:
                direction = wallNormal;
                if (_moveAxis.x > 0.5f || _moveAxis.x < -0.5f) {
                    direction = Quaternion.AngleAxis(90f, transform.up) * direction * Mathf.Sign(lastWallInput.x) * -1f;
                }
                if (prePopPosition != Vector3.zero) {
                    Vector3 prepopDelta = prePopPosition - transform.position;
                    if (prepopDelta.magnitude > 0.01f) {
                        Vector3 returningPosition = Vector3.Lerp(transform.position, prePopPosition, 0.5f);
                        Motor.SetPosition(returningPosition);
                    } else {
                        prePopPosition = Vector3.zero;
                    }
                }

                CheckUncrouch();
                break;
            case CharacterState.jumpPrep:
                jumpIndicatorController.transform.rotation = Quaternion.identity;
                direction = Motor.CharacterForward;
                Motor.SetCapsuleDimensions(defaultRadius, 0.4f, 0.2f);
                break;
            case CharacterState.climbing:
                switch (_climbingState) {
                    case ClimbingState.Climbing:
                        // Detect getting off ladder during climbing
                        _activeLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                        if (Mathf.Abs(_onLadderSegmentState) > 0.05f) {
                            _climbingState = ClimbingState.DeAnchoring;

                            // If we're higher than the ladder top point
                            if (_onLadderSegmentState > 0) {
                                _ladderTargetPosition = _activeLadder.TopReleasePoint.position;
                                _ladderTargetRotation = _activeLadder.TopReleasePoint.rotation;
                            }
                            // If we're lower than the ladder bottom point
                            else if (_onLadderSegmentState < 0) {
                                _ladderTargetPosition = _activeLadder.BottomReleasePoint.position;
                                _ladderTargetRotation = _activeLadder.BottomReleasePoint.rotation;
                            }
                        }
                        break;
                    case ClimbingState.Anchoring:
                    case ClimbingState.DeAnchoring:
                        // Detect transitioning out from anchoring states
                        if (_anchoringTimer >= AnchoringDuration) {
                            if (_climbingState == ClimbingState.Anchoring) {
                                _climbingState = ClimbingState.Climbing;
                            } else if (_climbingState == ClimbingState.DeAnchoring) {
                                TransitionToState(CharacterState.normal);
                            }
                        }

                        // Keep track of time since we started anchoring
                        _anchoringTimer += deltaTime;
                        break;
                }
                break;

        }
        // tookDamageThisFrame = false;
        if (hitstunTimer > 0f) {
            hitstunTimer -= Time.deltaTime;
        }
        if (gunHitStunTimer > 0f) {
            gunHitStunTimer -= Time.deltaTime;
        }
        if (GameManager.I.showDebugRays)
            Debug.DrawRay(transform.position + new Vector3(0f, 1f, 0f), direction, Color.red);
        recoil = Vector3.zero;
    }

    void CreateCorpse() {
        GameObject corpseObject = GameObject.Instantiate(Resources.Load("prefabs/gibs/corpse"), transform.position + new Vector3(0f, 0.83f, 0f), Quaternion.identity) as GameObject;
        Corpse corpse = corpseObject.GetComponent<Corpse>();
        // TODO: initialize corpse.
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
        // TODO: consider breaking out by state

        // We can wall jump only if we are not stable on ground and are moving against an obstruction
        if (AllowWallJump && !Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable) {
            _canWallJump = true;
            _wallJumpNormal = hitNormal;
        }

        // use Ray 1 here ?
        if (Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable && Vector3.Dot(_moveInputVector, hitNormal) < -0.9 && Vector3.Dot(_moveInputVector, hitNormal) > -1.1) {
            // wallPress = true;
            wallNormal = hitNormal;
            if (GameManager.I.showDebugRays)
                Debug.DrawRay(transform.position + new Vector3(0f, 1f, 0f), wallNormal, Color.red, 2f);
        }

        if (hitCollider.CompareTag("door")) {
            // Debug.Log($"pushing door: {hitCollider} {hitNormal} {hitPoint} {hitStabilityReport}");
            Door door = hitCollider.GetComponent<Door>();
            door.Push(hitNormal, hitPoint);
        }
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
    }

    public void PostGroundingUpdate(float deltaTime) {
    }

    public void AddVelocity(Vector3 velocity) {
        _internalVelocityAdd += velocity;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider) {
    }

    public void LoadState(PlayerState data) {
        superJumpEnabled = data.cyberlegsLevel > 0;
        thirdGunSlotEnabled = data.thirdWeaponSlot;
    }

    public CameraInput BuildCameraInput() {
        Vector2 finalLastWallInput = wallPressRatchet ? lastWallInput : Vector2.zero;

        CameraInput input = new CameraInput {
            deltaTime = Time.deltaTime,
            wallNormal = wallNormal,
            lastWallInput = finalLastWallInput,
            crouchHeld = isCrouching,
            playerPosition = transform.position,
            state = state,
            targetData = lastTargetDataInput,
            playerDirection = direction,
            playerLookDirection = lookAtDirection,
            popoutParity = popoutParity,
            aimCameraRotation = aimCameraRotation,
            targetTransform = cameraFollowTransform,
            targetPosition = cameraFollowTransform.position,
            atLeftEdge = atLeftEdge,
            atRightEdge = atRightEdge
        };
        return input;
    }

    public AnimationInput BuildAnimationInput() {
        // update view
        Vector2 camDir = new Vector2(OrbitCamera.Transform.forward.x, OrbitCamera.Transform.forward.z);
        Vector2 playerDir = new Vector2(direction.x, direction.z);

        // direction angles
        float angle = Vector2.SignedAngle(camDir, playerDir);

        // if (GameManager.I.showDebugRays)
        //     Debug.DrawRay(OrbitCamera.Transform.position, OrbitCamera.Transform.forward, Color.blue, 1f);

        return new AnimationInput {
            orientation = Toolbox.DirectionFromAngle(angle),
            isMoving = isMoving(),
            isCrouching = isCrouching,
            isProne = isProne,
            isRunning = isRunning,
            isJumping = state == CharacterState.superJump,
            isClimbing = state == CharacterState.climbing,
            wallPressTimer = wallPressTimer,
            state = state,
            playerInputs = _lastInput,
            gunInput = gunHandler.BuildAnimationInput(),
            camDir = camDir,
            cameraRotation = OrbitCamera.transform.rotation,
            lookAtDirection = lookAtDirection,
            movementSticking = IsMovementSticking(),
            directionToCamera = OrbitCamera.Transform.position - transform.position,
            hitState = hitState,
            velocity = Motor.Velocity
        };
    }
    bool IsMovementSticking() => (_lastInput.MoveAxis() != Vector2.zero && inputDirectionHeldTimer < crawlStickiness * 1.2f && isCrouching);
    public bool isMoving() {
        return Motor.Velocity.magnitude > 0.1 && (Motor.GroundingStatus.IsStableOnGround || state == CharacterState.climbing);
    }
    void LateUpdate() {
        OnValueChanged?.Invoke(this);
    }

    public void OnPoolActivate() {
    }
    public void OnPoolDectivate() {
        deadTimer = 0f;
        TransitionToState(CharacterState.normal);
        hitState = HitState.normal;
        landStunTimer = 0f;
        _lastInput = PlayerInput.none;
        lastTargetDataInput = CursorData.none;
        lookAtDirection = Vector3.zero;
        inputDirectionHeldTimer = 0f;
        crouchMovementInputTimer = 0f;
        crouchMovementSoundTimer = 0f;
        inputCrouchDown = false;
        deadMoveVelocity = Vector3.zero;
        deadTimer = 0f;
        hitstunTimer = 0f;
        gunHitStunTimer = 0f;
    }
}
