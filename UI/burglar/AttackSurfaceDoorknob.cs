using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackSurfaceDoorknob : AttackSurfaceElement {
    public Door door;
    public DoorLock doorLock;
    public AudioSource audioSource;
    public AudioClip[] pickSounds;
    public AudioClip[] manipulateSounds;
    public AudioClip[] keySounds;
    public float integratedPickTime;
    bool clickedThisFrame;
    public bool isHandle;
    public float setbackProb = 1;
    bool setback;

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.probe) {
            Toolbox.RandomizeOneShot(audioSource, manipulateSounds);
        } else if (activeTool == BurglarToolType.key) {
            bool success = false;
            foreach (int keyId in GameManager.I.gameData.playerState.physicalKeys) {
                success |= doorLock.TryKeyUnlock(DoorLock.LockType.physical, keyId);
            }
            Toolbox.RandomizeOneShot(audioSource, keySounds);
            if (success) {
                bool locked = doorLock.locked;
                return new BurglarAttackResult {
                    success = true,
                    feedbackText = locked ? $"{elementName} locked" : $"{elementName} unlocked"
                };
            } else {
                return new BurglarAttackResult {
                    success = false,
                    feedbackText = "Your keys don't work"
                };
            }
        } else if (activeTool == BurglarToolType.none && isHandle) {
            bool success = !door.IsLocked();
            door.ActivateDoorknob(data.burglar.transform.position);
            return BurglarAttackResult.None with {
                finish = success
            };
        }

        return BurglarAttackResult.None;
    }

    public override BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleClickHeld(activeTool, data);
        if (activeTool == BurglarToolType.lockpick && !setback) {
            clickedThisFrame = true;
            integratedPickTime += Time.deltaTime;
            door.PickJiggleKnob(doorLock);
            if (!audioSource.isPlaying) {
                Toolbox.RandomizeOneShot(audioSource, pickSounds);
            }

            float roll = Random.Range(0f, 1f);
            if (integratedPickTime > 0.2f && roll < Time.deltaTime * setbackProb) {
                setback = true;
                integratedPickTime *= Random.Range(0.25f, 0.75f);
                SetProgressPercent();
                OnValueChanged?.Invoke(this);
            }

            if (integratedPickTime > 2f) {
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
    }

    BurglarAttackResult DoPick() {
        bool success = doorLock.lockType == DoorLock.LockType.physical && doorLock.locked;
        doorLock.PickLock();
        if (success) {
            return new BurglarAttackResult {
                success = true,
                feedbackText = "Door unlocked"
            };
        } else return BurglarAttackResult.None;
    }

    void Update() {
        if (integratedPickTime > 0f) {
            if (!clickedThisFrame) {
                if (setback) {
                    integratedPickTime -= Time.deltaTime / 3f;
                } else {
                    integratedPickTime -= Time.deltaTime;
                }
            }
            SetProgressPercent();
            OnValueChanged?.Invoke(this);
        }
        clickedThisFrame = false;
    }

    void SetProgressPercent() {
        float baseline = (1.0f * progressStageIndex) / (1.0f * progressStages);
        float progress = (integratedPickTime / 2f) * (1f / (1.0f * progressStages));
        progressPercent = baseline + progress;
    }
}
