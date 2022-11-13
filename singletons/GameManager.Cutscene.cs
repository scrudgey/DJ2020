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
        StartCutsceneCoroutine(SpottedCutscene(NPC));
    }
    public IEnumerator SpottedCutscene(GameObject NPC) {
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
        yield return null;
    }

}