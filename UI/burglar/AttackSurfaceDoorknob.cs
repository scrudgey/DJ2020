using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceDoorknob : AttackSurfaceElement {
    public Door door;
    public AudioSource audioSource;
    public AudioClip[] pickSounds;
    public AudioClip[] manipulateSounds;
    public float integratedPickTime;
    bool clickedThisFrame;

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.probe) {
            Toolbox.RandomizeOneShot(audioSource, manipulateSounds);
        } else if (activeTool == BurglarToolType.none) {
            door.ActivateDoorknob(data.burglar.transform);
        }

        return BurglarAttackResult.None;
    }

    public override BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleClickHeld(activeTool, data);
        if (activeTool == BurglarToolType.lockpick) {
            clickedThisFrame = true;
            integratedPickTime += Time.deltaTime;
            door.PickJiggleKnob();
            if (!audioSource.isPlaying) {
                Toolbox.RandomizeOneShot(audioSource, pickSounds);
            }

            if (integratedPickTime > 2f) {
                integratedPickTime = 0f;
                progressPercent = 0f;
                OnValueChanged?.Invoke(this);
                return DoPick();
            }
        }
        return BurglarAttackResult.None;
    }

    BurglarAttackResult DoPick() {
        bool success = door.locked;
        door.Unlock();
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
                integratedPickTime -= Time.deltaTime;
            }
            progressPercent = integratedPickTime / 2f;
            OnValueChanged?.Invoke(this);
        }
        clickedThisFrame = false;
    }
}
