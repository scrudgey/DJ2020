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

    public void StartSpottedCutscene(GameObject NPC) {
        // foreach (SphereRobotAI ai in )
        if (GameObject.FindObjectsOfType<SphereRobotAI>().Any(ai => ai.stateMachine.currentState is SphereInvestigateState)) return;
        StartCutsceneCoroutine(SpottedCutscene(NPC));
    }
    public void ShowExtractionZoneCutscene(ExtractionZone zone) {
        StartCutsceneCoroutine(ExtractionZoneCutscene(zone));
    }
    public IEnumerator SpottedCutscene(GameObject NPC) {
        uiController?.HideUI();
        float timer = 0f;
        SphereRobotSpeaker speaker = NPC.GetComponentInChildren<SphereRobotSpeaker>();
        float distanceBetweenPeople = (NPC.transform.position - playerObject.transform.position).magnitude;

        Vector3 positionBetweenPeople = playerObject.transform.position + ((NPC.transform.position - playerObject.transform.position).normalized * distanceBetweenPeople / 2f);
        positionBetweenPeople += new Vector3(0f, 1f, 0f);

        if (speaker != null) {
            speaker.DoInvestigateSpeak();
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
                playerLookDirection = playerCharacterController.direction,
                popoutParity = PopoutParity.left,
                aimCameraRotation = Quaternion.identity,
                targetPosition = positionBetweenPeople
            };
            characterCamera.UpdateWithInput(input);
            yield return null;
        }

        speaker.speechTextController.HideText();
        Time.timeScale = 1f;
        characterCamera.followingSharpnessDefault = 5f;
        uiController?.ShowUI();
        yield return null;
    }

    public IEnumerator StartMissionCutscene() {
        // uiController?.HideUI();
        Time.timeScale = 0f;
        float timer = 0f;
        float duration = 3f;
        // characterCamera.followingSharpnessDefault = 5f;
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
                playerLookDirection = playerCharacterController.direction,
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
                playerLookDirection = playerCharacterController.direction,
                popoutParity = PopoutParity.left,
                aimCameraRotation = Quaternion.identity,
                targetPosition = zone.myCollider.bounds.center
            };
            characterCamera.UpdateWithInput(input, ignoreAttractor: true);
            yield return null;
        }
        Time.timeScale = 1f;
        characterCamera.followCursorCoefficient = 5f;
        yield return null;
    }


}