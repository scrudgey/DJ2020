using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class JumpIndicatorController : MonoBehaviour, IInputReceiver {
    public Transform indicator;
    public float moveRatio = 1f;
    public float superJumpSpeed;
    public Vector3 gravity;
    public void SetInputs(PlayerInput inputs) {
        // Clamp input
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);
        if (moveInputVector.y != 0 && moveInputVector.x != 0) {
            moveInputVector = CharacterCamera.rotationOffset * moveInputVector;
        }

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Vector3.up).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f) {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Vector3.up).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Vector3.up);

        // Move and look inputs
        moveInputVector = cameraPlanarRotation * moveInputVector * Time.deltaTime;
        if (inputs.MoveAxisRight != 0 && inputs.MoveAxisForward != 0) {
            moveInputVector = Quaternion.Inverse(CharacterCamera.rotationOffset) * moveInputVector;
        }

        // Move and look inputs
        indicator.position += moveInputVector * moveRatio;
        indicator.localPosition = Vector3.ClampMagnitude(
            indicator.localPosition,
            Toolbox.SuperJumpRange(superJumpSpeed, gravity.y));
    }
}