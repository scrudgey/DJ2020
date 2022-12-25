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
                if (totalRotation > 540) {
                    unscrewed = true;
                    return new BurglarAttackResult() {
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

    IEnumerator TurnScrew(float amount) {
        Vector3 initialEuler = transform.localRotation.eulerAngles;
        float finalZ = initialEuler.z + amount;
        float duration = 0.25f;
        float timer = 0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            float newZ = (float)PennerDoubleAnimation.Linear(timer, initialEuler.z, finalZ, duration);
            Quaternion newRotation = Quaternion.Euler(initialEuler.x, initialEuler.y, newZ);
            transform.localRotation = newRotation;
            yield return null;
        }
        turnCoroutine = null;
        if (totalRotation > 540) {
            screwImage.enabled = false;
            unscrewed = true;
        }
    }

}
