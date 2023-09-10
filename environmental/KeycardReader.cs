using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeycardReader : Interactive {
    public DoorLock doorLock;
    public AudioSource audioSource;
    public AudioClip[] successSound;
    public AudioClip[] failSound;
    public SpriteRenderer lightSprite;
    public Color successColor;
    public Color failColor;
    Coroutine coroutine;
    public override ItemUseResult DoAction(Interactor interactor) {
        bool success = false;
        foreach (int keyId in GameManager.I.gameData.playerState.keycards) {
            success |= doorLock.TryKeyUnlock(DoorLock.LockType.keycard, keyId);
        }
        AudioClip[] sound;
        Color color;
        if (success) {
            sound = successSound;
            color = successColor;
        } else {
            sound = failSound;
            color = failColor;
        }
        Toolbox.RandomizeOneShot(audioSource, sound);
        BlinkLight(color);
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }

    void BlinkLight(Color newColor) {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        lightSprite.color = newColor;
        coroutine = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine() {
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
    }

}
