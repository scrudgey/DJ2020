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
        characterCamera.followCursorCoefficient = 1f;
        Vector2 zoomInput = new Vector2(0f, 5f);
        yield return MoveCamera(zone.myCollider.bounds.center, Quaternion.identity, 2f, CameraState.normal, zoomInput, PennerDoubleAnimation.ExpoEaseOut);
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1f;
        characterCamera.followCursorCoefficient = 5f;

        yield return null;
    }
}