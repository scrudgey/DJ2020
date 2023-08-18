using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class AttackSurfaceScrew : AttackSurfaceElement {
    public AudioSource audioSource;
    public AudioClip[] screwSound;
    Coroutine turnCoroutine;
    float totalRotation;
    public SpriteRenderer screwImage;
    public bool unscrewed;
    readonly static float ROTATION_LIMIT = 360f;
    public override void Initialize(AttackSurfaceUIElement uIElement) {
        base.Initialize(uIElement);
        if (unscrewed) {
            screwImage.enabled = false;
            uiElement.gameObject.SetActive(false);
        }
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (unscrewed)
            return BurglarAttackResult.None;
        if (activeTool == BurglarToolType.screwdriver) {
            if (turnCoroutine == null) {
                Toolbox.RandomizeOneShot(audioSource, screwSound);
                float amount = Random.Range(90, 180f);
                totalRotation += amount;
                turnCoroutine = StartCoroutine(TurnScrew(amount));
                if (totalRotation > ROTATION_LIMIT) {
                    unscrewed = true;
                    uiElement.gameObject.SetActive(false);
                    return BurglarAttackResult.None with {
                        success = true,
                        feedbackText = "Screw removed",
                    };
                }
            }
        }
        return BurglarAttackResult.None;
    }

    public override BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleClickHeld(activeTool, data);
        return BurglarAttackResult.None;
    }
    // public override void Initialize(AttackSurfaceUIElement uIElement) {
    //     base.Initialize(uIElement);
    //     if (unscrewed) {
    //         Debug.Log($"disabling ui element {uiElement}");
    //         uiElement.gameObject.SetActive(false);
    //     }
    // }
    IEnumerator TurnScrew(float amount) {
        Vector3 initialEuler = transform.localRotation.eulerAngles;
        float finalZ = initialEuler.z - amount;     // fix this
        float duration = 0.25f;
        float timer = 0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            float newZ = (float)PennerDoubleAnimation.Linear(timer, initialEuler.z, amount, duration);
            Quaternion newRotation = Quaternion.Euler(initialEuler.x, initialEuler.y, newZ);
            transform.localRotation = newRotation;
            yield return null;
        }
        turnCoroutine = null;
        if (totalRotation > ROTATION_LIMIT) {
            screwImage.enabled = false;
            unscrewed = true;
        }
    }

}
