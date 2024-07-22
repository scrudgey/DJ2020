using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public abstract class Cutscene {
    public bool hasFocus;
    public CharacterCamera characterCamera;
    public CharacterController playerCharacterController;
    // public NeoClearsighterV4 clearsighter;
    public GameObject playerObject;

    readonly static Vector2 defaultZoomInput = Vector2.zero;


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

    protected void SetCameraPosition(Vector3 position, Quaternion rotation, CameraState state) {
        CameraInput input = new CameraInput {
            deltaTime = Time.unscaledDeltaTime,
            wallNormal = Vector2.zero,
            lastWallInput = Vector2.zero,
            crouchHeld = false,
            cameraState = state,
            targetData = CursorData.none,
            playerDirection = playerCharacterController.direction,
            popoutParity = PopoutParity.left,
            targetRotation = rotation,
            targetPosition = position,
            cullingTargetPosition = position,
            ignoreAttractor = true,
        };
        characterCamera.UpdateWithInput(input);
    }

    protected void SetCameraPosition(string idn, CameraState state) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        SetCameraPosition(data.transform.position, data.transform.rotation, state);
    }

    protected IEnumerator MoveCamera(string idn, float duration, CameraState state) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        yield return MoveCamera(data.transform.position, data.transform.rotation, duration, state);
    }
    protected IEnumerator MoveCamera(string idn, float duration, CameraState state, Func<double, double, double, double, double> easing) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        yield return MoveCamera(data.transform.position, data.transform.rotation, duration, state, Vector2.zero, easing);
    }
    protected IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration, CameraState state) {
        float timer = 0f;
        Vector3 initialPosition = characterCamera.transform.position;
        Quaternion initialRotation = characterCamera.transform.rotation;
        while (timer < duration) {

            float completion = timer / duration;

            Vector3 position = Vector3.Lerp(initialPosition, targetPosition, completion);
            Quaternion rotation = Quaternion.Lerp(initialRotation, targetRotation, completion);

            SetCameraPosition(position, rotation, state);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }
    protected IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration, CameraState state, Vector2 zoomInput) {
        return MoveCamera(targetPosition, targetRotation, duration, state, zoomInput, PennerDoubleAnimation.Linear);
    }
    protected IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration, CameraState state, Vector2 zoomInput, Func<double, double, double, double, double> easing) {
        float timer = 0f;
        Vector3 initialPosition = characterCamera.transform.position;
        Quaternion initialRotation = characterCamera.transform.rotation;
        while (timer < duration) {
            PlayerInput playerInput = PlayerInput.none;
            playerInput.zoomInput = zoomInput;
            characterCamera.SetInputs(playerInput);

            // float completion = timer / duration;
            float completion = (float)easing(timer, 0f, 1f, duration);

            Vector3 position = Vector3.Lerp(initialPosition, targetPosition, completion);
            Quaternion rotation = Quaternion.Lerp(initialRotation, targetRotation, completion);

            SetCameraPosition(position, rotation, state);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}