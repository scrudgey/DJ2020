using System.Collections;
using System.Collections.Generic;
using Easings;
using Obi;
using UnityEngine;


public class WireCutterToolIndicator : MonoBehaviour {
    public BurglarCanvasController burglarCanvasController;
    public LoHi leftAngles;
    public RectTransform toolTip;
    public RectTransform camImageTransform;
    public RectTransform leftHandle;
    public RectTransform rightHandle;
    public AudioSource audioSource;
    public AudioClip[] snipSound;
    public AudioClip[] wireCutSound;
    Coroutine snipRoutine;
    Quaternion initialLeftRotation;
    Quaternion initialRightRotation;
    BurglarCanvasController canvasController;
    void Start() {
        initialLeftRotation = leftHandle.rotation;
        initialRightRotation = rightHandle.rotation;
    }

    public void DoSnip(BurglarCanvasController canvasController, AttackSurface attackSurface, Vector2 position) {
        this.canvasController = canvasController;
        if (snipRoutine != null) {
            StopCoroutine(snipRoutine);
        }
        snipRoutine = StartCoroutine(Snip(attackSurface, position));
    }

    IEnumerator Snip(AttackSurface attackSurface, Vector2 position) {
        float timer = 0f;
        int repetitions = 1;
        int index = 0;

        float duration = 0.1f;
        while (index < repetitions) {
            while (timer < duration) {
                timer += Time.unscaledDeltaTime;
                float angle = (float)PennerDoubleAnimation.CubicEaseIn(timer, leftAngles.low, leftAngles.high - leftAngles.low, duration);

                Quaternion leftRotation = Quaternion.Euler(0f, 0f, angle);
                Quaternion rightRotation = Quaternion.Euler(0f, 0f, -1f * angle);

                leftHandle.rotation = leftRotation * initialLeftRotation;
                rightHandle.rotation = rightRotation * initialRightRotation;
                yield return null;
            }
            timer = 0f;
            while (timer < duration) {
                timer += Time.unscaledDeltaTime;
                float angle = (float)PennerDoubleAnimation.CubicEaseIn(timer, leftAngles.high, leftAngles.low - leftAngles.high, duration);

                Quaternion leftRotation = Quaternion.Euler(0f, 0f, angle);
                Quaternion rightRotation = Quaternion.Euler(0f, 0f, -1f * angle);

                leftHandle.rotation = leftRotation * initialLeftRotation;
                rightHandle.rotation = rightRotation * initialRightRotation;
                yield return null;
            }
            BurglarAttackResult result = CutWires(attackSurface, position);
            if (result.electricDamage == null) {
                if (result.success) {
                    Toolbox.RandomizeOneShot(audioSource, wireCutSound);
                } else {
                    Toolbox.RandomizeOneShot(audioSource, snipSound);
                }
            }
            canvasController.HandleAttackResult(result);
            timer = 0f;
            index += 1;
            yield return null;
        }
        snipRoutine = null;
    }

    BurglarAttackResult CutWires(AttackSurface attackSurface, Vector2 position) {
        Ray projection = burglarCanvasController.MousePositionToAttackCamRay(position);
        BurglarAttackResult result = attackSurface.HandleRopeCutting(projection);
        return result;
    }
}
