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
        doorLock.locked = false;
        Toolbox.RandomizeOneShot(audioSource, successSound);
        BlinkLight(successColor);
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
