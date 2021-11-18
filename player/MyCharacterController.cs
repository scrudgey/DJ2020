using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class MyCharacterController : MonoBehaviour, ICharacterController {
    public struct PlayerCharacterInputs {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
    }
    public KinematicCharacterMotor Motor;
    void Start() {
        Motor.CharacterController = this;
    }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) { }

    public void BeforeCharacterUpdate(float deltaTime) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void AfterCharacterUpdate(float deltaTime) { }

    public bool IsColliderValidForCollisions(Collider coll) { return true; }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
}
