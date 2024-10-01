using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Easings;
using UnityEngine;
public abstract class Cutscene {
    static readonly Vector2 HALFSIES = Vector2.one / 2f;

    public bool hasFocus;
    public CharacterCamera characterCamera;
    public CharacterController playerCharacterController;
    public GameObject playerObject;
    readonly static Vector2 defaultZoomInput = Vector2.zero;
    public bool isPlaying;
    public InputProfile inputProfile = InputProfile.allowNone;
    LocationHighlight locationHighlight;
    public IEnumerator Play() {
        isPlaying = true;
        yield return DoCutscene();
        // request unfocus
        isPlaying = false;
        yield return LeaveFocus();
    }

    protected IEnumerator WaitForFocus() {
        // Debug.Log("start wait for focus");
        yield return CutsceneManager.I.RequestFocus(this);
        // Debug.Log($"wait until cutscene has focus {this}");
        yield return new WaitUntil(() => this.hasFocus);
        // Debug.Log($"got focus");
    }

    protected IEnumerator LeaveFocus() {
        yield return CutsceneManager.I.LeaveFocus(this);
    }

    public abstract IEnumerator DoCutscene();


    protected IEnumerator WaitForTrigger(string idn) {
        bool trigger = false;
        Action<string> callback = (string triggerId) => {
            trigger |= triggerId == idn;
            // Debug.Log($"process incoming trigger: {triggerId}, {triggerId} == {idn}\t trigger: {trigger}");
        };
        CutsceneManager.OnTrigger += callback;
        while (!trigger) {
            // Debug.Log($"waiting for cutscene  trigger: {idn}");
            yield return null;
        }
        CutsceneManager.OnTrigger -= callback;
    }

    protected void SetCameraPosition(Vector3 position, Quaternion rotation, CameraState state, float orthographicSize = 1f, bool snapToOrthographicSize = false, bool snapToPosition = false, bool debug = false) {
        CursorData targetData = CursorData.none;
        targetData.screenPositionNormalized = HALFSIES;


        CameraInput input = new CameraInput {
            deltaTime = Time.unscaledDeltaTime,
            wallNormal = Vector2.zero,
            lastWallInput = Vector2.zero,
            crouchHeld = false,
            cameraState = state,
            targetData = targetData,
            playerDirection = playerCharacterController.direction,
            popoutParity = PopoutParity.left,
            targetRotation = rotation,
            targetPosition = position,
            cullingTargetPosition = position,
            ignoreAttractor = true,
            orthographicSize = orthographicSize,
            snapToOrthographicSize = snapToOrthographicSize,
            snapTo = snapToPosition
        };
        characterCamera.transitionTime = 1f;
        characterCamera.UpdateWithInput(input, debug: debug);
    }

    protected void SetCameraPosition(string idn, CameraState state, float orthographicSize = 1f, bool snapToOrthographicSize = false, bool snapToPosition = false) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        SetCameraPosition(data.transform.position, data.transform.rotation, state, orthographicSize: orthographicSize, snapToOrthographicSize: snapToOrthographicSize, snapToPosition: snapToPosition);
    }

    protected IEnumerator MoveCamera(string idn, float duration, CameraState state) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        yield return MoveCamera(data.transform.position, data.transform.rotation, duration, state);
    }
    protected IEnumerator MoveCamera(string idn, float duration, CameraState state, Func<double, double, double, double, double> easing) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        yield return MoveCamera(data.transform.position, data.transform.rotation, duration, state, Vector2.zero, easing);
    }
    protected IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration, CameraState state, bool debug = false, float buffer = 0.1f) {
        float timer = 0f;
        Vector3 initialPosition;
        if (state == CameraState.normal) {
            CutsceneManager.I.playerListener.enabled = true;
            CutsceneManager.I.cameraListener.enabled = false;
            initialPosition = characterCamera.lastTargetPosition;
        } else {

            CutsceneManager.I.playerListener.enabled = false;
            CutsceneManager.I.cameraListener.enabled = true;
            initialPosition = characterCamera.transform.position;
        }
        Quaternion initialRotation = characterCamera.transform.rotation;
        while (timer < duration) {

            float completion = timer / duration;

            Vector3 position = Vector3.Lerp(initialPosition, targetPosition, completion);
            Quaternion rotation = Quaternion.Lerp(initialRotation, targetRotation, completion);
            // Debug.Log($"{timer}\t{duration}\t{completion}\t{targetPosition} -> {position}");
            SetCameraPosition(position, rotation, state);
            timer += Time.unscaledDeltaTime;
            // Debug.Break();
            yield return null;
        }
        yield return new WaitForSecondsRealtime(buffer);
        SetCameraPosition(targetPosition, targetRotation, state, debug: debug);
    }
    protected IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration, CameraState state, Vector2 zoomInput) {
        return MoveCamera(targetPosition, targetRotation, duration, state, zoomInput, PennerDoubleAnimation.Linear);
    }
    protected IEnumerator MoveCamera(string idn, float duration, CameraState state, Vector2 zoomInput, Func<double, double, double, double, double> easing) {
        ScriptSceneCameraPosition data = CutsceneManager.I.cameraLocations[idn];
        return MoveCamera(data.transform.position, data.transform.rotation, duration, state, zoomInput, easing);
    }
    protected IEnumerator RotateIsometricCamera(IsometricOrientation desiredOrientation, Vector3 targetPosition) {
        PlayerInput playerInput = PlayerInput.none;
        playerInput.rotateCameraLeftPressedThisFrame = true;
        CursorData targetData = CursorData.none;
        targetData.screenPositionNormalized = HALFSIES;

        while (characterCamera.currentOrientation != desiredOrientation) {
            characterCamera.SetInputs(playerInput);

            float timer = 0;
            while (timer < 0.1f) {
                CameraInput input = new CameraInput {
                    deltaTime = Time.unscaledDeltaTime,
                    wallNormal = Vector2.zero,
                    lastWallInput = Vector2.zero,
                    crouchHeld = false,
                    cameraState = CameraState.normal,
                    targetData = targetData,
                    playerDirection = playerCharacterController.direction,
                    popoutParity = PopoutParity.left,
                    ignoreAttractor = true,
                    targetPosition = targetPosition,
                    cullingTargetPosition = targetPosition,
                    // snapTo = true 
                };
                // characterCamera.transitionTime = 1f;
                characterCamera.UpdateWithInput(input);
                // Debug.Log($"cam orientation: {characterCamera.currentOrientation} {desiredOrientation}");
                timer += Time.unscaledDeltaTime;
                characterCamera.SetInputs(PlayerInput.none);
                yield return null;
            }

            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.2f);
    }
    protected IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration, CameraState state, Vector2 zoomInput, Func<double, double, double, double, double> easing) {
        float timer = 0f;
        Vector3 initialPosition = characterCamera.lastTargetPosition;
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

    protected IEnumerator CameraIsometricZoom(float targetOrthographicSize, float duration) {
        yield return characterCamera.DoZoom(targetOrthographicSize, duration);
    }

    protected IEnumerator MoveCharacter(CharacterController controller, string key, float speedCoefficient = 1f, bool crawling = false, bool cameraFollows = false) {
        ScriptSceneLocation data = CutsceneManager.I.worldLocations[key];
        TaskMoveToKey task = new TaskMoveToKey(controller.transform, "walkToKey", new HashSet<int>(), controller);
        task.speedCoefficient = speedCoefficient;
        task.SetData("walkToKey", data.transform.position);
        task.Initialize();

        TaskState result = TaskState.failure;
        PlayerInput input = PlayerInput.none;
        while (result != TaskState.success) {
            result = task.DoEvaluate(ref input);
            if (crawling) {
                input.CrouchDown = true;
            }
            controller.SetInputs(input);
            if (cameraFollows) {
                CameraInput camInput = new CameraInput {
                    deltaTime = Time.unscaledDeltaTime,
                    wallNormal = Vector2.zero,
                    lastWallInput = Vector2.zero,
                    crouchHeld = false,
                    cameraState = CameraState.normal,
                    targetData = CursorData.none,
                    playerDirection = playerCharacterController.direction,
                    popoutParity = PopoutParity.left,
                    ignoreAttractor = true,
                    targetPosition = controller.transform.position + Vector3.up,
                    cullingTargetPosition = controller.transform.position,
                };
                characterCamera.UpdateWithInput(camInput);
                characterCamera.SetInputs(PlayerInput.none);
            }
            yield return null;
        }
    }

    protected void SelectItem(CharacterController controller) {
        // PlayerInput input = PlayerInput.none;
        // input.selectItem = -1;
        ItemHandler handler = controller.GetComponentInChildren<ItemHandler>();
        handler.ClearItem();
    }

    protected void CharacterLookAt(CharacterController controller, string key) {
        ScriptSceneLocation data = CutsceneManager.I.worldLocations[key];
        Vector3 orientTowardPoint = data.transform.position;
        orientTowardPoint.y = controller.transform.position.y;
        PlayerInput input = PlayerInput.none;
        input.lookAtPosition = data.transform.position;
        input.orientTowardPoint = orientTowardPoint;
        input.snapToLook = true;
        controller.SetInputs(input);
    }

    protected void HighlightLocation(string idn) {
        if (locationHighlight == null) {
            GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/cutsceneLocationHighlight")) as GameObject;
            locationHighlight = obj.GetComponent<LocationHighlight>();
        }
        ScriptSceneLocation data = CutsceneManager.I.worldLocations[idn];
        locationHighlight.target = data.transform.position;
        locationHighlight.gameObject.SetActive(true);
    }
    protected void HideLocationHighlight() {
        if (locationHighlight != null) {
            locationHighlight.gameObject.SetActive(false);
        }
    }


}
