using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public class AttackSurfaceLockToggle : AttackSurfaceElement {
    public DoorLock doorLock;
    public AudioSource audioSource;
    public AudioClip[] toggleSounds;
    public Transform toggleRotator;
    public Coroutine toggleCoroutine;
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            Toolbox.RandomizeOneShot(audioSource, toggleSounds);
            StartCoroutine();
            doorLock.locked = !doorLock.locked;
            return BurglarAttackResult.None with {
                success = true,
                feedbackText = doorLock.locked ? $"{doorLock.gameObject.name} locked" : $"{doorLock.gameObject.name} unlocked"
            };
        }
        return BurglarAttackResult.None;
    }
    void StartCoroutine() {
        if (toggleCoroutine != null) {
            StopCoroutine(toggleCoroutine);
        }
        toggleCoroutine = StartCoroutine(DoTurnKnobRoutine(toggleRotator));
    }
    IEnumerator DoTurnKnobRoutine(Transform knob) {
        float targetAngle = doorLock.locked ? 0f : 90f;
        Func<float> increment = doorLock.locked ? () => Time.deltaTime * -360f : () => Time.deltaTime * 360f;
        Func<float, bool> stopCondition = doorLock.locked ? (currentAngle) => currentAngle > 350f : (currentAngle) => currentAngle > 90f && currentAngle < 100f;

        if (knob == null) {
            yield return null;
        } else {
            float currentAngle = knob.localRotation.eulerAngles.z;
            while (!stopCondition(currentAngle)) {
                currentAngle = knob.localRotation.eulerAngles.z;
                Quaternion newRotation = Quaternion.Euler(0f, 0f, currentAngle + increment());
                knob.localRotation = newRotation;
                yield return null;
            }
        }
    }
}