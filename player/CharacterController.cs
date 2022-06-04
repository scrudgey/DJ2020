using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;

public enum CharacterState {
    normal,
    wallPress,
    climbing,
    jumpPrep,
    superJump,
    landStun,
}
public enum ClimbingState {
    Anchoring,
    Climbing,
    DeAnchoring
}

public class CharacterController : MonoBehaviour, ICharacterController, ISaveable, IBindable<CharacterController>, IInputReceiver {
    public KinematicCharacterMotor Motor;
    public CharacterCamera OrbitCamera;
    public JumpIndicatorController jumpIndicatorController;
    public GunHandler gunHandler;
    public ItemHandler itemHandler;
    public ManualHacker manualHacker;
    public Footsteps footsteps;
    public AudioSource audioSource;
    public float defaultRadius = 0.25f;

    public Action<CharacterController> OnValueChanged { get; set; }

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15;
    public float OrientationSharpness = 10;
    [Header("Crawling")]
    public float crawlSpeedFraction = 1f;
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
    public bool superJumpEnabled;

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
    private Vector3 _lookInputVector;
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
    private Vector3 _shootLookDirection = Vector2.zero;
    public bool isCrouching = false;
    public bool isRunning = false;

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
    public CharacterState state {
        get { return _state; }
    }

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
    private PlayerInput _lastInput;
    private TargetData2 lastTargetDataInput;

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
        }
    }
    public void OnStateExit(CharacterState state, CharacterState toState) {
        switch (state) {
            default:
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
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        // Assign to motor
        Motor.CharacterController = this;
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
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(input.MoveAxisRight, 0f, input.MoveAxisForward), 1f);
        if (moveInputVector.y != 0 && moveInputVector.x != 0) {
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

        // TODO: rotate input
        _inputTorque = input.torque;

        // Run input
        if (input.runDown) {
            if (!isRunning) {
                isRunning = true;
            }
        } else {
            isRunning = false;

            // Crouching input
            if (input.CrouchDown || input.jumpHeld) {
                if (!isCrouching) {
                    isCrouching = true;
                    Motor.SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
                }
            }
        }

        if (moveInputVector == Vector3.zero && input.jumpHeld && Motor.GroundingStatus.IsStableOnGround) {
            jumpHeldTimer += Time.deltaTime;
            if (superJumpEnabled && jumpHeldTimer > jumpTimerThreshold && state != CharacterState.jumpPrep) {
                TransitionToState(CharacterState.jumpPrep);
            }
        } else {
            jumpHeldTimer = 0;
        }

        TargetData2 targetData = input.Fire.targetData;
        switch (state) {
            case CharacterState.jumpPrep:
                // TODO: normalize this player state
                jumpIndicatorController.superJumpSpeed = superJumpSpeed;
                jumpIndicatorController.gravity = Gravity;
                jumpIndicatorController.SetInputs(input);
                if (input.jumpReleased) {
                    _timeSinceJumpRequested = 0f;
                    _jumpRequested = true;
                }
                break;
            default:
            case CharacterState.normal:
                wallPressRatchet = false;

                // Items
                itemHandler.SetInputs(input);

                // Cyberdeck
                ManualHackInput manualHackInput = new ManualHackInput {
                    playerInput = input,
                    activeItem = itemHandler.activeItem
                };
                manualHacker.SetInputs(manualHackInput);

                // Fire
                gunHandler.ProcessGunSwitch(input);
                gunHandler.SetInputs(input);

                Vector3 directionToCursor = targetData.position - transform.position;

                _lookInputVector = Vector3.zero;

                // turn to face aim position?
                if (input.Fire.AimPressed) {
                    _shootLookDirection = directionToCursor;
                    // Debug.Log($"setting aimpress shoot look direction: {_shootLookDirection}");
                }
                Debug.DrawRay(transform.position, directionToCursor, Color.yellow);
                if (GameManager.I.inputMode != InputMode.aim) {
                    directionToCursor.y = 0;
                    directionToCursor = directionToCursor.normalized;
                    float dotproduct = Vector3.Dot(Motor.CharacterForward, directionToCursor);
                    if (dotproduct < 0 && moveInputVector == Vector3.zero) {
                        // _lookInputVector = Vector3.Lerp(_lookInputVector, directionToCursor, 0.1f);
                    } else {
                        _lookInputVector = Vector3.Lerp(_lookInputVector, _moveInputVector, 0.1f);
                    }
                    if (targetData != TargetData2.none && (input.Fire.FireHeld || input.Fire.FirePressed)) {
                        _shootLookDirection = directionToCursor;
                    }
                }

                // Jumping input
                if (input.jumpReleased) {
                    _timeSinceJumpRequested = 0f;
                    _jumpRequested = true;
                }

                break;
            case CharacterState.wallPress:
                // allow gun switch
                gunHandler.ProcessGunSwitch(input);

                Motor.SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
                if (_moveAxis != Vector2.zero) {
                    lastWallInput = _moveAxis;
                } else {
                    wallPressRatchet = true;
                }
                if (!wallPressRatchet) {
                    _moveInputVector = Vector3.zero;
                    _moveAxis = Vector2.zero;
                }
                break;
            case CharacterState.climbing:
                if (input.jumpReleased) {
                    _timeSinceJumpRequested = 0f;
                    _jumpRequested = true;
                }
                break;
            case CharacterState.superJump:
                if (Motor.Velocity.y < 0) {
                    Motor.SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
                } else {
                    Motor.SetCapsuleDimensions(defaultRadius, 0.5f, 0.75f);
                }
                break;
        }

        _lastInput = input;
        lastTargetDataInput = targetData;
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

        switch (state) {
            default:
            case CharacterState.normal:
                if (_shootLookDirection != Vector3.zero) {
                    Vector3 target = _shootLookDirection;
                    target.y = 0;
                    currentRotation = Quaternion.LookRotation(target, Vector3.up);
                    // Debug.Log($"applying shoot look direction: {_shootLookDirection}");
                    // Debug.Break();
                } else if (wallPressTimer > 0 && wallNormal != Vector3.zero) { // wall pressing
                    // Smoothly interpolate from current to target look direction
                    Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, wallNormal, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                    // Set the current rotation (which will be used by the KinematicCharacterMotor)
                    // currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Vector3.up);
                } else if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f) {
                    // Smoothly interpolate from current to target look direction
                    Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                    // Set the current rotation (which will be used by the KinematicCharacterMotor)
                    currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Vector3.up);
                }
                Quaternion torqueRotation = Quaternion.FromToRotation(new Vector3(1f, 0f, 0f), new Vector3(Mathf.Cos(_inputTorque.y), 0f, Mathf.Sin(_inputTorque.y)));
                currentRotation = torqueRotation * currentRotation;
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
        _lookInputVector = Vector3.zero;
        _shootLookDirection = Vector3.zero;
    }
    bool ColliderRay(Vector3 offset) {
        // TODO: filter on layer
        Vector3 start = transform.position + offset;
        Vector3 dir = -1f * wallNormal;
        float length = 0.4f;
        // Debug.DrawRay(start, length * dir);
        foreach (RaycastHit hit in Physics.RaycastAll(start, dir, length).OrderBy(x => x.distance)) {
            if (hit.collider.transform.IsChildOf(transform)) {
                continue;
            }
            return true;
        }
        return false;
    }
    bool DetectWallPress() {
        if (GameManager.I.inputMode == InputMode.aim)
            return false;
        if (wallNormal == Vector3.zero)
            return false;
        return ColliderRay(new Vector3(0f, wallPressHeight, 0f));
    }
    bool AtRightEdge() {
        if (wallNormal == Vector3.zero)
            return false;
        return !ColliderRay(new Vector3(0f, wallPressHeight, 0f) + 0.2f * Motor.CharacterRight);
    }
    bool AtLeftEdge() {
        if (wallNormal == Vector3.zero)
            return false;
        return !ColliderRay(new Vector3(0f, wallPressHeight, 0f) + -0.2f * Motor.CharacterRight);
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
        bool pressingOnWall = DetectWallPress();
        Vector3 targetMovementVelocity = Vector3.zero;
        switch (state) {
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
            case CharacterState.wallPress:
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                // wall style input is relative to the wall normal
                if (AtRightEdge()) {
                    _moveAxis.x = Mathf.Max(0, _moveAxis.x);
                }
                if (AtLeftEdge()) {
                    _moveAxis.x = Mathf.Min(0, _moveAxis.x);
                }
                targetMovementVelocity = (Vector3.Cross(wallNormal, Motor.GroundingStatus.GroundNormal) * _moveAxis.x - wallNormal * _moveAxis.y) * MaxStableMoveSpeed * 0.5f;

                // transition from wall press 
                if (!pressingOnWall) {
                    TransitionToState(CharacterState.normal);
                }

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));

                break;
            case CharacterState.landStun:
                if (Motor.GroundingStatus.IsStableOnGround) {
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                    currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
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
            default:
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

                    if (GameManager.I.inputMode == InputMode.aim) {
                        targetMovementVelocity /= 2f;
                    } else if (isRunning) {
                        targetMovementVelocity *= runSpeedFraction;
                    } else if (isCrouching) {
                        // crawling
                        targetMovementVelocity *= crawlSpeedFraction;
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
                    footsteps.UpdateWithVelocity(currentVelocity);

                break;
            case CharacterState.climbing:
                currentVelocity = Vector3.zero;

                switch (_climbingState) {
                    case ClimbingState.Climbing:
                        currentVelocity = (_ladderUpDownInput * _activeLadder.transform.up).normalized * ClimbingSpeed;
                        footsteps.SetFootstepSounds(_activeLadder.surfaceType);
                        footsteps.UpdateWithVelocity(currentVelocity);
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
        if (isCrouching) {
            // Do an overlap test with the character's standing height to see if there are any obstructions
            Motor.SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
            _probedColliders = new Collider[8];
            if (Motor.CharacterCollisionsOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders) > 0) {
                // If obstructions, just stick to crouching dimensions
                Motor.SetCapsuleDimensions(defaultRadius, 1f, 0.5f);
                // Debug.Log("cancel the uncrouch");
            } else {
                // If no obstructions, uncrouch
                Motor.SetCapsuleDimensions(defaultRadius, 1.5f, 0.75f);
                isCrouching = false;
                // Debug.Log("uncrouching");
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
            default:
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
                CheckUncrouch();
                break;
            case CharacterState.jumpPrep:
                jumpIndicatorController.transform.rotation = Quaternion.identity;
                direction = Motor.CharacterForward;
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
        if (GameManager.I.showDebugRays)
            Debug.DrawRay(transform.position + new Vector3(0f, 1f, 0f), direction, Color.red);

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

    public void LoadState(PlayerData data) {
        superJumpEnabled = data.cyberlegsLevel > 0;
    }

    public CameraInput BuildCameraInput() {
        Vector2 finalLastWallInput = lastWallInput;
        if (!wallPressRatchet) {
            finalLastWallInput = Vector2.zero;
        }

        CameraInput input = new CameraInput {
            deltaTime = Time.deltaTime,
            wallNormal = wallNormal,
            lastWallInput = finalLastWallInput,
            crouchHeld = isCrouching,
            playerPosition = transform.position,
            state = state,
            targetData = lastTargetDataInput,
            playerDirection = direction
        };
        return input;
    }

    public AnimationInput BuildAnimationInput() {
        // update view
        Vector2 camDir = new Vector2(OrbitCamera.Transform.forward.x, OrbitCamera.Transform.forward.z);
        Vector2 playerDir = new Vector2(direction.x, direction.z);

        // head direction
        TargetData2 targetData = OrbitCamera.GetTargetData();

        // direction angles
        float angle = Vector2.SignedAngle(camDir, playerDir);

        // if (GameManager.I.showDebugRays)
        //     Debug.DrawRay(OrbitCamera.Transform.position, OrbitCamera.Transform.forward, Color.blue, 1f);

        // GunType gunType = GunType.unarmed;
        // Gun baseGun = null;
        // if (gunHandler.HasGun()) {
        //     gunType = gunHandler.gunInstance.baseGun.type;
        //     baseGun = gunHandler.gunInstance.baseGun;
        // }

        return new AnimationInput {
            orientation = Toolbox.DirectionFromAngle(angle),
            isMoving = isMoving(),
            isCrouching = isCrouching,
            isRunning = isRunning,
            isJumping = state == CharacterState.superJump,
            isClimbing = state == CharacterState.climbing,
            wallPressTimer = wallPressTimer,
            state = state,
            playerInputs = _lastInput,
            gunInput = gunHandler.BuildAnimationInput(),
            targetData = targetData,
            camDir = camDir
        };
    }
    public bool isMoving() {
        return Motor.Velocity.magnitude > 0.1 && (Motor.GroundingStatus.IsStableOnGround || state == CharacterState.climbing);
    }
    void LateUpdate() {
        OnValueChanged?.Invoke(this);
    }
}
