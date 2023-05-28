using System.Collections;
using System.Collections.Generic;
using Easings;
using Obi;
using UnityEngine;


public class WireCutterToolIndicator : MonoBehaviour {
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

    public void DoSnip(BurglarCanvasController canvasController, AttackSurface attackSurface) {
        this.canvasController = canvasController;
        if (snipRoutine != null) {
            StopCoroutine(snipRoutine);
        }
        snipRoutine = StartCoroutine(Snip(attackSurface));
    }

    IEnumerator Snip(AttackSurface attackSurface) {
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
            CutWires(attackSurface);
            timer = 0f;
            index += 1;
            yield return null;
        }

        snipRoutine = null;
    }

    void CutWires(AttackSurface attackSurface) {
        // Vector3 cursorPoint = new Vector3(input.mousePosition.x, input.mousePosition.y, data.target.attackCam.nearClipPlane);
        Vector3 cursorPoint = toolTip.anchoredPosition;
        cursorPoint -= camImageTransform.position;

        cursorPoint.z = attackSurface.attackCam.nearClipPlane;
        Ray projection = attackSurface.attackCam.ScreenPointToRay(cursorPoint);
        BurglarAttackResult result = attackSurface.HandleRopeCutting(projection);
        if (result != BurglarAttackResult.None) {
            Toolbox.RandomizeOneShot(audioSource, wireCutSound);
        } else {
            Toolbox.RandomizeOneShot(audioSource, snipSound);
        }
        canvasController.HandleAttackResult(result);
    }
}
