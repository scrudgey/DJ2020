using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class AttackSurfacePanelHandle : AttackSurfaceElement {
    public Transform hingeTransform;
    public LoHi hingeAngles;
    public Coroutine swingRoutine;
    public bool shut = true;
    public AudioSource audioSource;
    public AudioClip[] openSound;
    public AudioClip[] shutSound;
    public AudioClip[] lockedSound;
    public List<DoorLock> doorLocks;

    public List<DoorLock> getDoorLocks() => doorLocks;

    void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            if (!IsLocked()) {
                ToggleDoor();
                return BurglarAttackResult.None with {
                    success = true,
                    feedbackText = "opened access door",
                    element = this,
                    revealTamperEvidence = !shut,
                    hideTamperEvidence = shut
                };
            } else {
                HandleLockedDoor();
                List<Vector3> lockPositions = new List<Vector3>();
                foreach (DoorLock doorLock in doorLocks) {
                    if (doorLock.locked && doorLock.rotationElements != null) {
                        foreach (Transform rotation in doorLock.rotationElements) {
                            lockPositions.Add(rotation.position);
                        }
                    }
                }
                return BurglarAttackResult.None with {
                    success = false,
                    feedbackText = "door is locked",
                    element = this,
                    lockPositions = lockPositions
                };
            }
        }
        return BurglarAttackResult.None;
    }
    void ToggleDoor() {
        float currentAngle = hingeTransform.eulerAngles.y;
        if (swingRoutine != null) {
            StopCoroutine(swingRoutine);
        }
        if (shut) {
            swingRoutine = StartCoroutine(SwingHinge(hingeAngles.high, hingeAngles.low));
            Toolbox.RandomizeOneShot(audioSource, openSound);
        } else {
            swingRoutine = StartCoroutine(SwingHinge(hingeAngles.low, hingeAngles.high, conclusionSound: shutSound));
        }
        shut = !shut;
    }
    void HandleLockedDoor() {
        Toolbox.RandomizeOneShot(audioSource, lockedSound);
        swingRoutine = StartCoroutine(Rattle());
    }

    IEnumerator SwingHinge(float startAngle, float targetAngle, AudioClip[] conclusionSound = null) {
        float delta = targetAngle - startAngle;
        float timer = 0f;
        float duration = 1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float angle = (float)PennerDoubleAnimation.CircEaseInOut(timer, startAngle, delta, duration);
            Quaternion newRotation = Quaternion.Euler(0f, angle, 0f);
            hingeTransform.localRotation = newRotation;
            yield return null;
        }
        if (conclusionSound != null)
            Toolbox.RandomizeOneShot(audioSource, conclusionSound);
        swingRoutine = null;
    }

    IEnumerator Rattle() {
        float timer = 0f;
        float duration = 0.5f;
        float startAngle = shut ? hingeAngles.high : hingeAngles.low;
        while (timer < duration / 2f) {
            timer += Time.unscaledDeltaTime;
            float angle = (float)PennerDoubleAnimation.BounceEaseOut(timer, startAngle, -3, duration / 2f);
            Quaternion newRotation = Quaternion.Euler(0f, angle, 0f);
            hingeTransform.localRotation = newRotation;
            yield return null;
        }
        timer = 0f;
        while (timer < duration / 2f) {
            timer += Time.unscaledDeltaTime;
            float angle = (float)PennerDoubleAnimation.BounceEaseOut(timer, startAngle - 3, 3, duration / 2f);
            Quaternion newRotation = Quaternion.Euler(0f, angle, 0f);
            hingeTransform.localRotation = newRotation;
            yield return null;
        }
        swingRoutine = null;
    }

    public bool IsLocked() => doorLocks.Any(doorLock => doorLock.locked); // doorLock.locked;

    public void PickJiggleKnob(DoorLock doorlock) {
        // if (knobs == null || knobs.Count() == 0)
        //     return;
        if (doorlock.rotationElements.Length == 0) return;
        foreach (Transform knob in doorlock.rotationElements) {
            if (knob == null) continue;
            if (!Door.knobCoroutines.ContainsKey(knob)) {
                Door.knobCoroutines[knob] = StartCoroutine(Door.PickJiggleKnobRoutine(knob));
            }
        }
    }
}
