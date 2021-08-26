using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

public class JumpIndicatorController : MonoBehaviour {
    public Transform indicator;
    public float moveRatio = 0.01f;

    // public void Start() {
    // indicator.localPosition = new Vector3(0f, 0.01f, 0f);
    // }
    public void SetInputs(ref PlayerCharacterInputs inputs, NeoCharacterController characterController) {
        if (inputs.state != CharacterState.jumpPrep)
            return;

        // Clamp input
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);
        if (moveInputVector.y != 0 && moveInputVector.x != 0) {
            moveInputVector = NeoCharacterCamera.rotationOffset * moveInputVector;
        }

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Vector3.up).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f) {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Vector3.up).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Vector3.up);

        // Move and look inputs
        moveInputVector = cameraPlanarRotation * moveInputVector;
        if (inputs.MoveAxisRight != 0 && inputs.MoveAxisForward != 0) {
            moveInputVector = Quaternion.Inverse(NeoCharacterCamera.rotationOffset) * moveInputVector;
        }

        // Move and look inputs
        // indicator.position = transform.position + moveInputVector + new Vector3(0f, 0.01f, 0f);
        indicator.position += moveInputVector * moveRatio;

        indicator.localPosition = Vector3.ClampMagnitude(
            indicator.localPosition,
            Toolbox.SuperJumpRange(characterController.superJumpSpeed, characterController.Gravity.y)
            );
    }
}