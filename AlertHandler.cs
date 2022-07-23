using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;

public class AlertHandler : MonoBehaviour {
    public RectTransform alertRect;
    // public TextMeshProUGUI textMesh;
    public SpriteRenderer spriteRenderer;
    public Material alertMaterial;
    public Material warnMaterial;
    private Coroutine coroutine;
    private Sprite[] alertSprites;
    void Awake() {
        spriteRenderer.enabled = false;
        // textMesh.enabled = false;
    }
    void Start() {
        alertSprites = Resources.LoadAll<Sprite>("sprites/UI/Alert") as Sprite[];
    }
    public void Hide() {
        spriteRenderer.enabled = false;
        // textMesh.enabled = false;
    }

    void ResetCoroutine(IEnumerator newCoroutine) {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        StartCoroutine(newCoroutine);
    }
    public void ShowAlert() {
        ResetCoroutine(ShowAlertIcon());
    }
    public void ShowWarn() {
        ResetCoroutine(ShowQuestionIcon());
    }
    public void ShowGiveUp() {
        // ResetCoroutine(ShowText("<sprite=9>"));
        ResetCoroutine(ShowQuestionIcon());
    }

    IEnumerator ShowAlertIcon() {
        float appearanceInterval = 0.25f;
        float timer = 0f;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = alertSprites[1];
        spriteRenderer.material = alertMaterial;
        while (timer < appearanceInterval) {
            timer += Time.deltaTime;
            Vector2 sizeDelta = new Vector2();
            sizeDelta.x = 1f;
            sizeDelta.y = (float)PennerDoubleAnimation.BackEaseOut(timer, 0f, 1f, appearanceInterval);
            alertRect.localScale = sizeDelta;
            yield return null;
        }
        while (timer < 2.5f) {
            timer += Time.deltaTime;
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
            timer += Time.deltaTime;
            Vector2 sizeDelta = new Vector2();
            sizeDelta.x = 1f;
            sizeDelta.y = (float)PennerDoubleAnimation.BackEaseOut(timer, 0f, 1f, appearanceInterval);
            alertRect.localScale = sizeDelta;
            yield return null;
        }
        while (timer < 2.5f) {
            timer += Time.deltaTime;
            yield return null;
        }
        alertRect.sizeDelta = Vector2.one;
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
