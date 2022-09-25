using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.AI;

public class SphereRobotController : MonoBehaviour, ICharacterController, IBindable<SphereRobotController>, IInputReceiver { //}, ISaveable {
    public GunHandler gunHandler;

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15;
    public float OrientationSharpness = 10;
    public Vector3 gravity = new Vector3(0, -30f, 0);
    public NavMeshAgent navMeshAgent;
    public KinematicCharacterMotor Motor;
    public Vector3 direction;
    Vector3 targetDirection;
    private void Start() {
        Motor.CharacterController = this;

        // TODO: remove?
        gunHandler.primary = GunState.Instantiate(GunTemplate.Load("s1"));
        gunHandler.SwitchToGun(1);
        gunHandler.Reload();
        gunHandler.ClipIn();
        gunHandler.Rack();
    }

    public Action<SphereRobotController> OnValueChanged { get; set; }
    private Vector3 _moveInputVector;
    // private Vector3 slewLookDirection;
    private Vector2 _moveAxis;
    private Vector3 snapLookDirection = Vector2.zero;
    public void SetInputs(PlayerInput input) {
        if (input.Fire.cursorData != CursorData.none && (input.Fire.FireHeld || input.Fire.FirePressed)) {
            Vector3 targetPoint = input.Fire.cursorData.worldPosition;
            snapLookDirection = targetPoint;
        } else if (input.orientTowardPoint != Vector3.zero) {
            snapLookDirection = input.orientTowardPoint - transform.position;
        } else if (input.orientTowardDirection != Vector3.zero) {
            snapLookDirection = input.orientTowardDirection;
        } else if (input.lookAtDirection != Vector3.zero) {
            snapLookDirection = input.lookAtDirection;
        } else if (input.lookAtPosition != Vector3.zero) {
            snapLookDirection = input.lookAtPosition - transform.position;
        }

        _moveInputVector = input.moveDirection;
        if (input.MoveAxisForward != 0 || input.MoveAxisRight != 0) {
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
        }

        input.Fire.skipAnimation = true;
        gunHandler.ProcessGunSwitch(input);
        gunHandler.SetInputs(input);
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

        // TODO: first thing to check

        if (_moveInputVector != Vector3.zero) {
            targetDirection = _moveInputVector;
        } else if (snapLookDirection != Vector3.zero) {
            // noise.transform.position - owner.transform.position - transform.position;
            targetDirection = snapLookDirection - transform.position;
            targetDirection.y = 0;
        }
        // Debug.DrawRay(transform.position, targetDirection, Color.green, 0.1f);

        direction = Vector3.Slerp(direction, targetDirection, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
        currentRotation = Quaternion.LookRotation(direction, Vector3.up);

        snapLookDirection = Vector3.zero;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        Vector3 targetMovementVelocity = Vector3.zero;

        Debug.DrawLine(transform.position, transform.position + _moveInputVector, Color.blue, 0.1f);

        // Calculate target velocity
        Vector3 inputRight = Vector3.Cross(_moveInputVector, Vector3.up);
        Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
        targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

        // Smooth movement Velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));

        // apply gravity
        if (!Motor.GroundingStatus.IsStableOnGround) {
            currentVelocity += gravity * deltaTime;
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime) {
        OnValueChanged?.Invoke(this);
    }


    public bool IsColliderValidForCollisions(Collider coll) {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
    }

    public void PostGroundingUpdate(float deltaTime) {
    }

    public void AddVelocity(Vector3 velocity) {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider) {
    }

}
