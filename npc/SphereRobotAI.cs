using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SphereRobotAI : MonoBehaviour {
    public NavMeshAgent navMeshAgent;
    public SphereRobotController sphereController;
    private float newDestinationTimer;
    void Update() {
        newDestinationTimer -= Time.deltaTime;
        if (newDestinationTimer <= 0) {
            SetDestination();
            newDestinationTimer = Random.Range(2f, 15f);
        }
        SetInputs();
    }
    void SetDestination() {
        Vector3 destination = 10f * UnityEngine.Random.insideUnitSphere;
        destination.y = 0;
        navMeshAgent.SetDestination(destination);
        // Debug.Log($"new destination: {destination}");
    }

    void SetInputs() {
        Vector3 inputVector = navMeshAgent.desiredVelocity.normalized;
        PlayerInput input = new PlayerInput() {
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

        sphereController.SetInputs(input);
    }
}
