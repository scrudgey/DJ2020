using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceKeycardReader : AttackSurfaceElement {
    public DoorLock doorLock;
    public ElevatorController elevatorController;
    public AudioSource audioSource;
    public AudioClip[] successSound;
    public AudioClip[] failSound;
    public SpriteRenderer successSprite;
    public SpriteRenderer failSprite;
    bool blinkIsRunning;

    void Start() {
        successSprite.enabled = false;
        failSprite.enabled = false;
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.keycard) {
            bool success = false;
            foreach (int keyId in GameManager.I.gameData.levelState.delta.keycards) {
                success |= doorLock.TryKeyUnlock(DoorLock.LockType.keycard, keyId);
            }
            if (success) {
                Toolbox.RandomizeOneShot(audioSource, successSound);
                elevatorController.EnableTemporaryAuthorization();
                BlinkSuccessLight();
                return BurglarAttackResult.None with {
                    success = true,
                    feedbackText = "success"
                };
            } else {
                Toolbox.RandomizeOneShot(audioSource, failSound);
                BlinkFailLight();
                return BurglarAttackResult.None with {
                    success = false,
                    feedbackText = "Your keys don't work"
                };
            }
        }

        return BurglarAttackResult.None;
    }

    public void BlinkSuccessLight() {
        BlinkLight(successSprite);
    }
    public void BlinkFailLight() {
        BlinkLight(failSprite);
    }

    void BlinkLight(SpriteRenderer lightSprite) {
        if (!blinkIsRunning) {
            StartCoroutine(BlinkRoutine(lightSprite));
        }
    }

    IEnumerator BlinkRoutine(SpriteRenderer lightSprite) {
        blinkIsRunning = true;
        float timer = 0f;
        bool enabled = true;
        float interval = 0.25f;
        int number = 0;
        lightSprite.enabled = enabled;
        while (number < 5) {
            timer += Time.deltaTime;
            if (timer > interval) {
                timer -= interval;
                number += 1;
                enabled = !enabled;
                lightSprite.enabled = enabled;
            }
            yield return null;
        }
        lightSprite.enabled = false;
        blinkIsRunning = false;
    }
}
