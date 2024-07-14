using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

class ExtractionZoneCutscene : Cutscene {
    ExtractionZone zone;
    public ExtractionZoneCutscene(ExtractionZone zone) {
        this.zone = zone;
    }

    public override IEnumerator DoCutscene() {
        yield return WaitForFocus();
        float timer = 0f;
        float duration = 4f;

        characterCamera.followCursorCoefficient = 1f;
        // clearsighter.followTransform = zone.transform;      // TODO: bind clearsighter to camera

        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0f, 0.01f);
            PlayerInput playerInput = PlayerInput.none;
            playerInput.zoomInput = new Vector2(0f, 5f);
            characterCamera.SetInputs(playerInput);

            SetCameraPosition(zone.myCollider.bounds.center, CameraState.normal);
            yield return null;
        }
        Time.timeScale = 1f;
        characterCamera.followCursorCoefficient = 5f;

        // clearsighter.followTransform = playerObject.transform;

        yield return null;
    }
}