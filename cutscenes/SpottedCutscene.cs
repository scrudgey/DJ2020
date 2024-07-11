using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

class SpottedCutscene : Cutscene {
    SphereRobotAI NPC;
    public SpottedCutscene(SphereRobotAI NPC) {
        this.NPC = NPC;
    }

    public override IEnumerator DoCutscene() {
        yield return WaitForFocus();

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

            SetCameraPosition(positionBetweenPeople, Quaternion.identity, CameraState.normal);
            yield return null;
        }

        NPC.speechTextController?.HideText();
        Time.timeScale = 1f;    // TODO
        characterCamera.followingSharpnessDefault = 5f;
        // uiController?.ShowUI();
        // yield return null;
    }
}