using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

public struct PlayerCharacterInputs {
    public struct FireInputs {
        public bool FirePressed;
        public bool FireHeld;
        public Vector2 cursorPosition;
    }
    public float MoveAxisForward;
    public float MoveAxisRight;
    public Quaternion CameraRotation;
    public bool JumpDown;
    public bool CrouchDown;
    public FireInputs Fire;
    public bool reload;
    public int switchToGun;
}

public class NeoCharacterController : MonoBehaviour, ICharacterController {
    public KinematicCharacterMotor Motor;
    public GunHandler gunHandler;

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15;
    public float OrientationSharpness = 10;
    [Header("Crawling")]
    public float crawlSpeedFraction = 1f;

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 10f;
    public float AirAccelerationSpeed = 5f;
    public float Drag = 0.1f;

    [Header("Jumping")]
    public bool AllowJumpingWhenSliding = false;
    public bool AllowDoubleJump = false;
    public bool AllowWallJump = false;
    public float JumpSpeed = 10f;
    public float JumpPreGroundingGraceTime = 0f;
    public float JumpPostGroundingGraceTime = 0f;

    [Header("Misc")]
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public Vector3 direction;
    private Collider[] _probedColliders = new Collider[8];
    private Vector2 _moveAxis;
    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private bool _doubleJumpConsumed = false;
    private bool _canWallJump = false;
    private Vector3 _wallJumpNormal;
    private Vector3 _internalVelocityAdd = Vector3.zero;
    private bool _shouldBeCrouching = false;
    private Vector3 _shootLookDirection = Vector2.zero;
    public bool isCrouching = false;
    public bool wallPress = false;

    public float wallPressTimer = 0f;
    public float wallPressThreshold = 0.5f;
    public bool wallPressRatchet = false;

    public Vector3 wallNormal = Vector3.zero;
    public Vector2 lastWallInput = Vector2.zero;

    private void Start() {
        // Assign to motor
        Motor.CharacterController = this;
    }

    /// <summary>
    /// This is called every frame by MyPlayer in order to tell the character what its inputs are
    /// </summary>
    public void SetInputs(ref PlayerCharacterInputs inputs) {
        // Clamp input
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f) {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        // Move and look inputs
        _moveInputVector = cameraPlanarRotation * moveInputVector;
        _moveAxis = new Vector2(inputs.MoveAxisRight, inputs.MoveAxisForward);

        // Fire
        _shootLookDirection = gunHandler.ProcessInput(inputs);

        _lookInputVector = Vector3.Lerp(_lookInputVector, moveInputVector, 0.1f);

        // Jumping input
        if (inputs.JumpDown) {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }

        // Crouching input
        if (inputs.CrouchDown) {
            _shouldBeCrouching = true;

            if (!isCrouching) {
                isCrouching = true;
                Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
            }
        } else {
            _shouldBeCrouching = false;
        }

        if (wallPress) {

            if (_moveAxis != Vector2.zero) {
                lastWallInput = _moveAxis;
            } else {
                wallPressRatchet = true;
            }
            if (!wallPressRatchet) {
                _moveInputVector = Vector3.zero;
                _moveAxis = Vector2.zero;
            }
        } else {
            wallPressRatchet = false;
        }
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
        if (_shootLookDirection != Vector3.zero) {
            Vector3 target = _shootLookDirection - transform.position;
            target.y = 0;
            currentRotation = Quaternion.LookRotation(target, Motor.CharacterUp);
        } else if (!wallPress && wallPressTimer > 0 && wallNormal != Vector3.zero) {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, wallNormal, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
        } else if (!wallPress && _lookInputVector != Vector3.zero && OrientationSharpness > 0f) {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _moveInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
        } else if (wallPress) {
            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(wallNormal, Motor.CharacterUp);
        }
        direction = Motor.CharacterForward;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {

        Vector3 targetMovementVelocity = Vector3.zero;
        if (Motor.GroundingStatus.IsStableOnGround) {
            // Reorient velocity on slope
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

            // TODO: implement state enum
            // TODO: tighten up the push angle requirement
            if (wallPress) {
                // wall style input is relative to the wall normal
                targetMovementVelocity = (Vector3.Cross(wallNormal, Motor.GroundingStatus.GroundNormal) * _moveAxis.x - wallNormal * _moveAxis.y) * MaxStableMoveSpeed * 0.5f;

                // if (Vector3.Dot(wallNormal, _moveAxis) > 0.02f) {
                if (_moveAxis.y < -0.02f) {
                    wallPress = false;
                }
            } else {
                if (Vector3.Dot(_moveInputVector, wallNormal) < -0.9 && Vector3.Dot(_moveInputVector, wallNormal) > -1.1) {
                    wallPressTimer += Time.deltaTime;
                    if (wallPressTimer > wallPressThreshold) {
                        wallPress = true;
                        wallPressTimer = 0;
                    }
                } else if (_moveInputVector == Vector3.zero) {
                    wallPressTimer = 0;
                    wallNormal = Vector3.zero;
                } else if (wallPressTimer > 0) {
                    wallPressTimer -= Time.deltaTime;
                }
                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;
                if (isCrouching) {
                    targetMovementVelocity *= crawlSpeedFraction;
                }
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
        {
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested) {
                // Handle double jump
                if (AllowDoubleJump) {
                    if (_jumpConsumed && !_doubleJumpConsumed && (AllowJumpingWhenSliding ? !Motor.GroundingStatus.FoundAnyGround : !Motor.GroundingStatus.IsStableOnGround)) {
                        Motor.ForceUnground(0.1f);

                        // Add to the return velocity and reset jump state
                        currentVelocity += (Motor.CharacterUp * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                        _jumpRequested = false;
                        _doubleJumpConsumed = true;
                        _jumpedThisFrame = true;
                    }
                }

                // See if we actually are allowed to jump
                if (_canWallJump ||
                    (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))) {
                    // Calculate jump direction before ungrounding
                    Vector3 jumpDirection = Motor.CharacterUp;
                    if (_canWallJump) {
                        jumpDirection = _wallJumpNormal;
                    } else if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround) {
                        jumpDirection = Motor.GroundingStatus.GroundNormal;
                    }

                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    Motor.ForceUnground(0.1f);

                    // Add to the return velocity and reset jump state
                    currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                    _jumpRequested = false;
                    _jumpConsumed = true;
                    _jumpedThisFrame = true;
                }
            }

            // Reset wall jump
            _canWallJump = false;
        }

        // Take into account additive velocity
        if (_internalVelocityAdd.sqrMagnitude > 0f) {
            currentVelocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }

    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime) {
        // Handle jump-related values
        {
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
        }

        // Handle uncrouching
        if (isCrouching && !_shouldBeCrouching) {
            // Do an overlap test with the character's standing height to see if there are any obstructions
            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
            if (Motor.CharacterCollisionsOverlap(
                    Motor.TransientPosition,
                    Motor.TransientRotation,
                    _probedColliders) > 0) {
                // If obstructions, just stick to crouching dimensions
                Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
            } else {
                // If no obstructions, uncrouch
                MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                isCrouching = false;
            }
        }
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
        // We can wall jump only if we are not stable on ground and are moving against an obstruction
        if (AllowWallJump && !Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable) {
            _canWallJump = true;
            _wallJumpNormal = hitNormal;
        }
        if (Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable && Vector3.Dot(_moveInputVector, hitNormal) < -0.9 && Vector3.Dot(_moveInputVector, hitNormal) > -1.1) {
            // wallPress = true;
            wallNormal = hitNormal;
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
}
