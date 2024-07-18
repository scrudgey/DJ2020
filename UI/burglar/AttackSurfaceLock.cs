using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;

public class AttackSurfaceLock : AttackSurfaceElement {
    public AudioSource audioSource;
    public DoorLock doorLock;

    public AudioClip[] pickSounds;
    public AudioClip[] manipulateSounds;
    public AudioClip[] keySounds;
    public AudioClip[] pinSetSound;
    public AudioClip[] setbackSound;
    public AudioClip[] finishSound;
    public float integratedPickTime;
    public float setbackProb = 1;
    bool setback;
    bool clickedThisFrame;
    public static Dictionary<Transform, Coroutine> knobCoroutines = new Dictionary<Transform, Coroutine>();
    public Transform[] rotationElements;

    // int playerLockpickLevel;
    float lockPickSkillCoefficient;
    void Start() {
        SetSkillCoefficient();
    }

    void SetSkillCoefficient() {
        int playerLockpickLevel = GameManager.I.gameData.playerState.PerkLockpickLevel();
        lockPickSkillCoefficient = playerLockpickLevel switch {
            0 => 1f,
            1 => 1.7f,
            2 => 2f,
            3 => 3f,
            _ => 1f
        };
    }

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        SetSkillCoefficient();
        if (activeTool == BurglarToolType.probe) {
            Toolbox.RandomizeOneShot(audioSource, manipulateSounds);
        } else if (activeTool == BurglarToolType.key) {
            bool success = false;
            foreach (KeyData keyData in GameManager.I.gameData.levelState.delta.keys.Where(key => key.type == KeyType.physical)) {
                success |= doorLock.TryKeyUnlock(keyData);
            }
            Toolbox.RandomizeOneShot(audioSource, keySounds);
            if (success) {
                bool locked = doorLock.locked;
                return BurglarAttackResult.None with {
                    success = true,
                    feedbackText = locked ? $"{elementName} locked" : $"{elementName} unlocked"
                };
            } else {
                return BurglarAttackResult.None with {
                    success = false,
                    feedbackText = "Your keys don't work"
                };
            }
        } else if (activeTool == BurglarToolType.lockpick) {
            if (!doorLock.locked) {
                return BurglarAttackResult.None with {
                    success = true,
                    feedbackText = "Already unlocked"
                };
            }
        }

        return BurglarAttackResult.None;
    }

    public override BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleClickHeld(activeTool, data);
        if (activeTool == BurglarToolType.lockpick && !setback && doorLock.locked) {
            clickedThisFrame = true;
            integratedPickTime += Time.deltaTime * lockPickSkillCoefficient;
            PickJiggleKnob(doorLock);
            if (!audioSource.isPlaying) {
                Toolbox.RandomizeOneShot(audioSource, pickSounds);
            }

            float roll = Random.Range(0f, 1f);
            if (integratedPickTime > 0.2f && roll < Time.deltaTime * setbackProb) {
                setback = true;
                resetToolJiggle = true;
                integratedPickTime = 0;
                SetProgressPercent();
                audioSource.Stop();
                OnValueChanged?.Invoke(this);
                Toolbox.RandomizeOneShot(audioSource, setbackSound);
            }
            // Debug.Log($"{progressStageIndex} >= {progressStages}");

            if (integratedPickTime > 2f) {
                audioSource.Stop();
                CompleteProgress();
                OnValueChanged?.Invoke(this);
                if (progressStageIndex >= progressStages)
                    return DoPick();
            }
        }
        return BurglarAttackResult.None;
    }
    public override void HandleMouseUp() {
        base.HandleMouseUp();
        this.resetToolJiggle = false;
        SetProgressPercent();
        setback = false;
        OnValueChanged?.Invoke(this);
    }
    public override void HandleFocusLost() {
        base.HandleFocusLost();
        setback = false;
        progressStageIndex = 0;
        progressPercent = 0;
        SetProgressPercent();
        OnValueChanged?.Invoke(this);
    }
    void CompleteProgress() {
        integratedPickTime = 0f;
        progressPercent = 0f;
        progressStageIndex += 1;
        // TODO: play sound
        Toolbox.RandomizeOneShot(audioSource, pinSetSound);
    }
    void SetProgressPercent() {
        progressPercent = (integratedPickTime / 2f);
    }

    protected virtual BurglarAttackResult DoPick() {
        bool success = doorLock.lockType == KeyType.physical && doorLock.locked;
        doorLock.PickLock();
        if (success) {
            complete = true;
            audioSource.Stop();
            Toolbox.AudioSpeaker(transform.position, finishSound);
            OnValueChanged?.Invoke(this);
            return BurglarAttackResult.None with {
                success = true,
                feedbackText = "Door unlocked"
            };
        } else return BurglarAttackResult.None;
    }

    public void PickJiggleKnob(DoorLock doorlock) {
        if (rotationElements.Length == 0) return;
        foreach (Transform knob in rotationElements) {
            if (knob == null) continue;
            if (!knobCoroutines.ContainsKey(knob)) {
                knobCoroutines[knob] = StartCoroutine(PickJiggleKnobRoutine(knob));
            }
        }
    }

    void Update() {
        if (integratedPickTime > 0f) {
            if (!clickedThisFrame) {
                if (setback) {
                    // integratedPickTime -= Time.deltaTime / 3f;
                } else {
                    integratedPickTime -= Time.deltaTime;
                }
            }
            SetProgressPercent();
            OnValueChanged?.Invoke(this);
        }
        clickedThisFrame = false;
    }
    public static IEnumerator PickJiggleKnobRoutine(Transform knob) {
        float timer = 0f;
        float duration = Random.Range(0.05f, 0.15f);
        float startAngle = knob.localRotation.eulerAngles.z;
        if (startAngle > 180) startAngle -= 360f;
        float offset = Random.Range(-10f, 10f);
        float finalAngle = Mathf.Clamp(startAngle + offset, -10f, 10f);
        while (timer < duration) {
            float turnAngle = (float)PennerDoubleAnimation.CircEaseIn(timer, startAngle, finalAngle - startAngle, duration);
            Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
            knob.localRotation = turnRotation;
            timer += Time.deltaTime;
            yield return null;
        }
        knobCoroutines.Remove(knob);
    }
}
