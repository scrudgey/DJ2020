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

        Rigidbody grate = element.grate;

        // Vector3 positiion = element.transform.position - 2f * Vector3.up;
        Vector3 positiion = element.transform.position;
        Vector3 direction = element.transform.position - characterCamera.transform.position;
        Vector3 up = Vector3.Cross(characterCamera.transform.right, direction);
        Quaternion rotation = Quaternion.LookRotation(direction, up);

        float timer = 0f;
        while (timer < 0.2f) {
            timer += Time.deltaTime;
            SetCameraPosition(positiion, rotation, CameraState.free);
            yield return null;
        }
        // kick
        element.PlayImactSound();
        yield return Toolbox.ShakeTree(element.transform, Quaternion.identity);

        timer = 0f;
        while (timer < 1.2f) {
            timer += Time.deltaTime;
            SetCameraPosition(positiion, rotation, CameraState.free);
            yield return null;
        }
        // kick
        element.PlayImactSound();
        yield return Toolbox.ShakeTree(element.transform, Quaternion.identity);
        timer = 0f;
        while (timer < 1.2f) {
            timer += Time.deltaTime;
            SetCameraPosition(positiion, rotation, CameraState.free);
            yield return null;
        }
        // eject
        element.PlayEjectSound();

        grate.isKinematic = false;
        Vector3 force = UnityEngine.Random.Range(-4750f, -6525f) * Vector3.up + UnityEngine.Random.Range(2500f, 4700f) * Vector3.right + UnityEngine.Random.Range(2500f, 4700f) * Vector3.forward;
        Vector3 torque = UnityEngine.Random.Range(3550f, 4250f) * Vector3.right + UnityEngine.Random.Range(3550f, 4250f) * Vector3.forward;
        grate.AddForce(force);
        grate.AddTorque(torque);
        playerCharacterController.TransitionToState(CharacterState.normal);

        GameObject.Destroy(grate.gameObject, 5f);
    }
}