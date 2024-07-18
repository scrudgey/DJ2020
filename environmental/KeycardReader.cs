using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeycardReader : Interactive {
    public DoorLock doorLock;
    public AudioSource audioSource;
    public AudioClip[] successSound;
    public AudioClip[] failSound;
    public SpriteRenderer lightSprite;
    public Color successColor;
    public Color failColor;
    public HeavyDoor heavyDoor;
    public SlidingDoor slidingDoor;
    Coroutine coroutine;
    public override ItemUseResult DoAction(Interactor interactor) {
        AttemptToOpenDoors();
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }
    public void AttemptToOpenDoors() {
        bool success = TryUnlock();
        if (success) {
            if (heavyDoor != null) {
                heavyDoor.OpenDoors();
            }
            if (slidingDoor != null) {
                slidingDoor.OpenDoors();
            }
        }
    }

    public bool TryUnlock() {
        bool success = false;
        foreach (KeyData keyData in GameManager.I.gameData.levelState.delta.keys.Where(key => key.type == KeyType.keycard)) {
            success |= doorLock.TryKeyUnlock(keyData);
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
        return success;
    }
    public bool AttemptSingleKey(KeyData keyData) {
        bool success = doorLock.TryKeyUnlock(keyData);
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
        return success;
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
