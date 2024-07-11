using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public abstract class Cutscene {
    public bool hasFocus;
    public CharacterCamera characterCamera;
    public CharacterController playerCharacterController;
    public NeoClearsighterV4 clearsighter;
    public GameObject playerObject;


    public IEnumerator Play() {
        yield return DoCutscene();
        // request unfocus
        CutsceneManager.I.LeaveFocus(this);
    }

    protected IEnumerator WaitForFocus() {
        CutsceneManager.I.RequestFocus(this);
        yield return new WaitUntil(() => this.hasFocus);
    }

    public abstract IEnumerator DoCutscene();

    protected void SetCameraPosition(Vector3 position, CameraState state) {
        CameraInput input = new CameraInput {
            deltaTime = Time.unscaledDeltaTime,
            wallNormal = Vector2.zero,
            lastWallInput = Vector2.zero,
            crouchHeld = false,
            playerPosition = position,
            state = state,
            targetData = CursorData.none,
            playerDirection = playerCharacterController.direction,
            popoutParity = PopoutParity.left,
            aimCameraRotation = Quaternion.identity,
            targetPosition = position
        };
        characterCamera.UpdateWithInput(input);
        characterCamera.transform.position = position;
        // characterCamera.state = state;
    }
    protected void SetCameraPosition(Vector3 position, Quaternion rotation, CameraState state) {
        CameraInput input = new CameraInput {
            deltaTime = Time.unscaledDeltaTime,
            wallNormal = Vector2.zero,
            lastWallInput = Vector2.zero,
            crouchHeld = false,
            playerPosition = position,
            state = state,
            targetData = CursorData.none,
            playerDirection = playerCharacterController.direction,
            popoutParity = PopoutParity.left,
            aimCameraRotation = Quaternion.identity,
            targetPosition = position
        };
        characterCamera.UpdateWithInput(input);
        characterCamera.transform.position = position;
        characterCamera.transform.rotation = rotation;
        // characterCamera.state = state;
    }

    protected void SetCameraPosition(string idn) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        CameraInput input = new CameraInput {
            deltaTime = Time.unscaledDeltaTime,
            wallNormal = Vector2.zero,
            lastWallInput = Vector2.zero,
            crouchHeld = false,
            playerPosition = data.transform.position,
            state = CameraState.free,
            targetData = CursorData.none,
            playerDirection = playerCharacterController.direction,
            popoutParity = PopoutParity.left,
            aimCameraRotation = data.transform.rotation,
            targetPosition = data.transform.position
        };
        characterCamera.UpdateWithInput(input);
        characterCamera.transform.position = data.transform.position;
        characterCamera.transform.rotation = data.transform.rotation;
    }

    protected IEnumerator MoveCamera(string idn, float duration) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];

        float timer = 0f;
        Vector3 initialPosition = characterCamera.transform.position;
        Quaternion initialRotation = characterCamera.transform.rotation;
        while (timer < duration) {

            float completion = timer / duration;

            Vector3 position = Vector3.Lerp(initialPosition, data.transform.position, completion);
            Quaternion rotation = Quaternion.Lerp(initialRotation, data.transform.rotation, completion);

            SetCameraPosition(position, rotation, CameraState.free);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        characterCamera.transform.position = data.transform.position;
        characterCamera.transform.rotation = data.transform.rotation;
    }
}