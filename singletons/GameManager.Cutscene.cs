using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public partial class GameManager : Singleton<GameManager> {
    public Coroutine cutscene;
    bool cutsceneIsRunning;

    void StartCutsceneCoroutine(IEnumerator newCoroutine) {
        if (cutscene != null) {
            StopCoroutine(cutscene);
        }
        cutscene = StartCoroutine(CutsceneWrapper(newCoroutine));
    }

    IEnumerator CutsceneWrapper(IEnumerator wrapped) {
        cutsceneIsRunning = true;
        yield return StartCoroutine(wrapped);
        cutsceneIsRunning = false;
    }

    public void StartSpottedCutscene(SphereRobotAI NPC) {
        if (GameObject.FindObjectsOfType<SphereRobotAI>().Any(ai => ai.stateMachine.currentState is SphereInvestigateState)) return;
        StartCutsceneCoroutine(SpottedCutscene(NPC));
    }
    public void ShowExtractionZoneCutscene(ExtractionZone zone) {
        StartCutsceneCoroutine(ExtractionZoneCutscene(zone));
    }
    public void ShowGrateKickCutscene(HVACElement element, CharacterController controller) {
        StartCutsceneCoroutine(KickOutHVACGrateCutscene(element, controller));
    }
    public IEnumerator SpottedCutscene(SphereRobotAI NPC) {
        uiController?.HideUI();
        float timer = 0f;
        float distanceBetweenPeople = (NPC.transform.position - playerObject.transform.position).magnitude;

        Vector3 positionBetweenPeople = playerObject.transform.position + ((NPC.transform.position - playerObject.transform.position).normalized * distanceBetweenPeople / 2f);
        positionBetweenPeople += new Vector3(0f, 1f, 0f);

        if (NPC.speechTextController != null) {
            NPC.speechTextController.SaySpotted();
        }

        characterCamera.followingSharpnessDefault = 1f;
        while (timer < 2f) {
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0f, 0.01f);
            if (characterCamera.Camera.orthographicSize > distanceBetweenPeople / 2f) {
                float pseudoTime = characterCamera.Camera.orthographicSize - distanceBetweenPeople / 2f;
                pseudoTime = Math.Max(0, pseudoTime);
                pseudoTime = Math.Min(5, pseudoTime);
                float coefficient = (float)PennerDoubleAnimation.CircEaseIn(pseudoTime, 0f, 10f, 5f);

                PlayerInput playerInput = PlayerInput.none;
                playerInput.zoomInput = new Vector2(0f, 1f) * coefficient;
                characterCamera.SetInputs(playerInput);
            }

            CameraInput input = new CameraInput {
                deltaTime = Time.deltaTime,
                wallNormal = Vector2.zero,
                lastWallInput = Vector2.zero,
                crouchHeld = false,
                playerPosition = transform.position,
                state = CharacterState.normal,
                targetData = CursorData.none,
                playerDirection = playerCharacterController.direction,
                // playerLookDirection = playerCharacterController.direction,
                popoutParity = PopoutParity.left,
                aimCameraRotation = Quaternion.identity,
                targetPosition = positionBetweenPeople
            };
            characterCamera.UpdateWithInput(input);
            yield return null;
        }

        NPC.speechTextController?.HideText();
        Time.timeScale = 1f;
        characterCamera.followingSharpnessDefault = 5f;
        uiController?.ShowUI();
        yield return null;
    }

    public IEnumerator StartMissionCutscene() {
        Time.timeScale = 0f;
        float timer = 0f;
        float duration = 3f;
        characterCamera.zoomCoefficient = 0.25f;
        characterCamera.zoomCoefficientTarget = 0.25f;
        Vector3 targetPosition = playerObject.transform.position;
        characterCamera._currentFollowPosition = targetPosition;
        while (timer < duration) {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, 0.01f);
            float coefficient = (float)PennerDoubleAnimation.CircEaseIn(timer, 0f, 10f, duration);

            PlayerInput playerInput = PlayerInput.none;
            playerInput.zoomInput = new Vector2(0f, -1f) * coefficient;
            characterCamera.SetInputs(playerInput);

            CameraInput input = new CameraInput {
                deltaTime = Time.deltaTime,
                wallNormal = Vector2.zero,
                lastWallInput = Vector2.zero,
                crouchHeld = false,
                playerPosition = targetPosition,
                state = CharacterState.normal,
                targetData = CursorData.none,
                playerDirection = playerCharacterController.direction,
                // playerLookDirection = playerCharacterController.direction,
                popoutParity = PopoutParity.left,
                aimCameraRotation = Quaternion.identity,
                targetPosition = targetPosition
            };
            characterCamera.UpdateWithInput(input, ignoreAttractor: true);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        yield return null;
    }

    public IEnumerator ExtractionZoneCutscene(ExtractionZone zone) {
        float timer = 0f;
        float duration = 4f;
        characterCamera.followCursorCoefficient = 1f;
        // clearSighter2.followTransform = zone.transform;
        clearSighterV3.followTransform = zone.transform;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0f, 0.01f);
            PlayerInput playerInput = PlayerInput.none;
            playerInput.zoomInput = new Vector2(0f, 5f);
            characterCamera.SetInputs(playerInput);
            CameraInput input = new CameraInput {
                deltaTime = Time.deltaTime,
                wallNormal = Vector2.zero,
                lastWallInput = Vector2.zero,
                crouchHeld = false,
                playerPosition = zone.myCollider.bounds.center,
                state = CharacterState.normal,
                targetData = CursorData.none,
                playerDirection = playerCharacterController.direction,
                // playerLookDirection = playerCharacterController.direction,
                popoutParity = PopoutParity.left,
                aimCameraRotation = Quaternion.identity,
                targetPosition = zone.myCollider.bounds.center
            };
            characterCamera.UpdateWithInput(input, ignoreAttractor: true);
            yield return null;
        }
        Time.timeScale = 1f;
        characterCamera.followCursorCoefficient = 5f;

        // clearSighter2.followTransform = playerObject.transform;
        clearSighterV3.followTransform = playerObject.transform;

        yield return null;
    }

    public IEnumerator KickOutHVACGrateCutscene(HVACElement element, CharacterController controller) {
        Rigidbody grate = element.grate;

        // Vector3 positiion = element.transform.position - 2f * Vector3.up;
        Vector3 positiion = element.transform.position;
        Vector3 direction = element.transform.position - characterCamera.transform.position;
        Vector3 up = Vector3.Cross(characterCamera.transform.right, direction);
        Quaternion rotation = Quaternion.LookRotation(direction, up);

        CameraInput input = new CameraInput {
            deltaTime = Time.deltaTime,
            wallNormal = Vector2.zero,
            lastWallInput = Vector2.zero,
            crouchHeld = false,
            playerPosition = positiion,
            state = CharacterState.normal,
            targetData = CursorData.none,
            playerDirection = playerCharacterController.direction,
            // playerLookDirection = playerCharacterController.direction,
            popoutParity = PopoutParity.left,
            aimCameraRotation = rotation,
            targetPosition = positiion
        };

        float timer = 0f;
        while (timer < 0.2f) {
            timer += Time.deltaTime;
            characterCamera.UpdateWithInput(input, ignoreAttractor: true);
            yield return null;
        }
        // kick
        element.PlayImactSound();
        StartCoroutine(Toolbox.ShakeTree(element.transform, Quaternion.identity));

        timer = 0f;
        while (timer < 1.2f) {
            timer += Time.deltaTime;
            characterCamera.UpdateWithInput(input, ignoreAttractor: true);
            yield return null;
        }
        // kick
        element.PlayImactSound();
        StartCoroutine(Toolbox.ShakeTree(element.transform, Quaternion.identity));
        timer = 0f;
        while (timer < 1.2f) {
            timer += Time.deltaTime;
            characterCamera.UpdateWithInput(input, ignoreAttractor: true);
            yield return null;
        }
        // eject
        element.PlayEjectSound();

        grate.isKinematic = false;
        Vector3 force = UnityEngine.Random.Range(-4750f, -6525f) * Vector3.up + UnityEngine.Random.Range(2500f, 4700f) * Vector3.right + UnityEngine.Random.Range(2500f, 4700f) * Vector3.forward;
        Vector3 torque = UnityEngine.Random.Range(3550f, 4250f) * Vector3.right + UnityEngine.Random.Range(3550f, 4250f) * Vector3.forward;
        grate.AddForce(force);
        grate.AddTorque(torque);
        controller.TransitionToState(CharacterState.normal);

        Destroy(grate.gameObject, 5f);
    }

}