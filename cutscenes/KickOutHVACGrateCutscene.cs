using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

class KickOutHVACGrateCutscene : Cutscene {
    HVACElement element;
    public KickOutHVACGrateCutscene(HVACElement element) {
        this.element = element;
    }

    public override IEnumerator DoCutscene() {
        yield return WaitForFocus();
        Time.timeScale = 1;

        Rigidbody grate = element.grate;

        yield return MoveCamera(element.cameraPosition.position, element.cameraPosition.rotation, 0.25f, CameraState.free);

        yield return new WaitForSecondsRealtime(0.2f);
        // kick
        element.PlayImactSound();
        yield return ShakeGrate(grate.transform);

        yield return new WaitForSecondsRealtime(1.2f);

        // kick
        element.PlayImactSound();
        yield return ShakeGrate(grate.transform);
        yield return new WaitForSecondsRealtime(1.2f);

        element.PlayEjectSound();

        grate.isKinematic = false;
        Vector3 force = UnityEngine.Random.Range(-1347.50f, -2565.25f) * Vector3.up + UnityEngine.Random.Range(25f, 80f) * Vector3.right + UnityEngine.Random.Range(25f, 90f) * Vector3.forward;
        Vector3 torque = UnityEngine.Random.Range(35.50f, 42.50f) * Vector3.right + UnityEngine.Random.Range(35.50f, 42.50f) * Vector3.forward;
        grate.AddForce(force, ForceMode.Impulse);
        grate.AddTorque(torque, ForceMode.Impulse);

        playerCharacterController.TransitionToState(CharacterState.normal);
        yield return new WaitForSecondsRealtime(1.2f);

        GameObject.Destroy(grate.gameObject, 5f);
    }

    IEnumerator ShakeGrate(Transform grate) {
        Vector3 initialPos = grate.position;
        Quaternion initialRot = grate.localRotation;
        float translateAmplitude = UnityEngine.Random.Range(0.01f, 0.2f);
        float rotationXAmplitude = UnityEngine.Random.Range(1f, 5f);
        float rotationYAmplitude = UnityEngine.Random.Range(1f, 5f);
        yield return Toolbox.Ease(null, 0.25f, 1f, 0f, PennerDoubleAnimation.BounceEaseOut, (amount) => {
            float translate = translateAmplitude * amount;
            float rotationX = rotationXAmplitude * amount;
            float rotationY = rotationYAmplitude * amount;
            grate.position = initialPos - (translate * Vector3.up);
            grate.localRotation = initialRot * Quaternion.Euler(rotationX, rotationY, 0f);
        }, unscaledTime: true);
        grate.position = initialPos;
        grate.localRotation = initialRot;
    }
}