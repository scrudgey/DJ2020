using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

class StartMissionCutscene : Cutscene {
    public StartMissionCutscene() {
    }

    public override IEnumerator DoCutscene() {
        yield return WaitForFocus();
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

            SetCameraPosition(targetPosition, Quaternion.identity, CameraState.free);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        yield return null;
    }
}