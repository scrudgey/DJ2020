using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class AttackSurfacePanelHandle : AttackSurfaceElement, IDoor {
    public Transform hingeTransform;
    public LoHi hingeAngles;
    public Coroutine swingRoutine;
    public bool shut = true;
    public AudioSource audioSource;
    public AudioClip[] openSound;
    public AudioClip[] shutSound;
    public List<DoorLock> doorLocks;

    void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            if (!IsLocked()) {
                ToggleDoor();
                return new BurglarAttackResult {
                    success = true,
                    feedbackText = "opened access door",
                    element = this
                };
            } else {
                return new BurglarAttackResult {
                    success = false,
                    feedbackText = "door is locked",
                    element = this
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

    public bool IsLocked() => doorLocks.Any(doorLock => doorLock.locked); // doorLock.locked;

    public void ActivateDoorknob(Vector3 position, Transform activator, HashSet<int> withKeySet = null, bool bypassKeyCheck = false, bool openOnly = false) {

    }

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
