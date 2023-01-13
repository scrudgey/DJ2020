using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;

public class AlertHandler : MonoBehaviour {
    public RectTransform alertRect;
    public SpriteRenderer spriteRenderer;
    public Material alertMaterial;
    public Material warnMaterial;
    private Coroutine coroutine;
    private Sprite[] alertSprites;
    static readonly float DISPLAY_TIME = 2f;
    void Awake() {
        spriteRenderer.enabled = false;
    }
    void Start() {
        alertSprites = Resources.LoadAll<Sprite>("sprites/UI/Alert") as Sprite[];
    }
    public void Hide() {
        spriteRenderer.enabled = false;
    }

    void ResetCoroutine(IEnumerator newCoroutine) {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(newCoroutine);
    }
    public void ShowAlert(bool useWarnMaterial = false) {
        // alertRect.localScale = Vector3.one * 2.5F;
        // alertRect.localPosition = new Vector3(0f, 2f, 0f);
        ResetCoroutine(ShowAlertIcon(useWarnMaterial: useWarnMaterial));
    }
    public void ShowWarn() {
        // alertRect.localScale = Vector3.one * 2.5F;
        // alertRect.localPosition = new Vector3(0f, 2f, 0f);

        ResetCoroutine(ShowQuestionIcon());
    }
    public void ShowGiveUp() {
        // alertRect.localScale = Vector3.one * 2.5F;
        // alertRect.localPosition = new Vector3(0f, 2f, 0f);
        // ResetCoroutine(ShowText("<sprite=9>"));
        ResetCoroutine(ShowQuestionIcon());
    }
    public void ShowRadio() {
        // alertRect.localScale = Vector3.one * 2f;
        // alertRect.localPosition = new Vector3(0f, 1.45f, 0f);
        ResetCoroutine(ShowRadioIcon());
    }
    public void HideRadio() {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        spriteRenderer.enabled = false;
        // alertRect.localScale = Vector3.one;
        // alertRect.localPosition = Vector3.one * 2f;
    }

    IEnumerator ShowAlertIcon(bool useWarnMaterial = false) {
        float appearanceInterval = 0.25f;
        float timer = 0f;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = alertSprites[1];
        spriteRenderer.material = useWarnMaterial ? warnMaterial : alertMaterial;
        while (timer < appearanceInterval) {
            timer += Time.unscaledDeltaTime;
            Vector2 sizeDelta = new Vector2();
            sizeDelta.x = 1f;
            sizeDelta.y = (float)PennerDoubleAnimation.BackEaseOut(timer, 0f, 1f, appearanceInterval);
            alertRect.localScale = sizeDelta;// * 2f;
            yield return null;
        }
        while (timer < DISPLAY_TIME) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        alertRect.sizeDelta = Vector2.one;
        spriteRenderer.enabled = false;
    }

    IEnumerator ShowQuestionIcon() {
        float appearanceInterval = 0.25f;
        float timer = 0f;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = alertSprites[5];
        spriteRenderer.material = warnMaterial;
        while (timer < appearanceInterval) {
            timer += Time.unscaledDeltaTime;
            Vector2 sizeDelta = new Vector2();
            sizeDelta.x = 1f;
            sizeDelta.y = (float)PennerDoubleAnimation.BackEaseOut(timer, 0f, 1f, appearanceInterval);
            alertRect.localScale = sizeDelta;
            yield return null;
        }
        while (timer < DISPLAY_TIME) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        alertRect.sizeDelta = Vector2.one;
        spriteRenderer.enabled = false;
    }

    IEnumerator ShowRadioIcon() {
        int alertIndex = 6;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = alertSprites[7];
        spriteRenderer.material = warnMaterial;
        float totalTimer = 0f;
        float intervalTimer = 0f;

        float totalDuration = 10f;
        float intervalDuration = 0.2f;
        while (totalTimer < totalDuration) {
            totalTimer += Time.unscaledDeltaTime;
            intervalTimer += Time.unscaledDeltaTime;
            if (intervalTimer > intervalDuration) {
                intervalTimer -= intervalDuration;
                alertIndex += 1;
                if (alertIndex > 9) {
                    alertIndex = 6;
                }
                if (alertIndex == 6) {
                    spriteRenderer.enabled = false;
                } else {
                    spriteRenderer.enabled = true;
                }
                spriteRenderer.sprite = alertSprites[alertIndex];
            }
            yield return null;
        }
        spriteRenderer.enabled = false;
    }

    // IEnumerator ShowText(string content) {
    //     float appearanceInterval = 0.25f;
    //     float timer = 0f;
    //     // alertIcon.enabled = true;
    //     textMesh.enabled = true;
    //     textMesh.text = content;
    //     while (timer < appearanceInterval) {
    //         timer += Time.deltaTime;
    //         Vector2 sizeDelta = new Vector2();
    //         sizeDelta.x = 1f;
    //         sizeDelta.y = (float)PennerDoubleAnimation.BackEaseOut(timer, 0f, 1f, appearanceInterval);
    //         // alertRect.localScale = sizeDelta;
    //         textMesh.transform.localScale = sizeDelta;
    //         yield return null;
    //     }
    //     while (timer < 2.5f) {
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //     textMesh.transform.localScale = Vector2.one;
    //     // alertIcon.enabled = false;
    //     // textMesh.transform.localScale =
    //     textMesh.enabled = false;
    // }


}
