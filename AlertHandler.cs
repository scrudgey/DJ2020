using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;

public class AlertHandler : MonoBehaviour {
    public RectTransform alertRect;
    public TextMeshProUGUI textMesh;
    public SpriteRenderer alertIcon;
    public Material alertMaterial;
    public Material warnMaterial;
    private Coroutine coroutine;
    void Awake() {
        alertIcon.enabled = false;
        textMesh.enabled = false;
    }
    public void Hide() {
        alertIcon.enabled = false;
        textMesh.enabled = false;
    }

    void ResetCoroutine(IEnumerator newCoroutine) {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        StartCoroutine(newCoroutine);
    }
    public void ShowAlert() {
        alertIcon.material = alertMaterial;
        ResetCoroutine(ShowAlertIcon());
    }
    public void ShowWarn() {
        alertIcon.material = warnMaterial;
        ResetCoroutine(ShowAlertIcon());
    }
    public void ShowGiveUp() {
        ResetCoroutine(ShowText("<sprite=9>"));
    }

    IEnumerator ShowAlertIcon() {
        float appearanceInterval = 0.25f;
        float timer = 0f;
        alertIcon.enabled = true;
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
        alertIcon.enabled = false;
    }

    IEnumerator ShowText(string content) {
        float appearanceInterval = 0.25f;
        float timer = 0f;
        // alertIcon.enabled = true;
        textMesh.enabled = true;
        textMesh.text = content;
        while (timer < appearanceInterval) {
            timer += Time.deltaTime;
            Vector2 sizeDelta = new Vector2();
            sizeDelta.x = 1f;
            sizeDelta.y = (float)PennerDoubleAnimation.BackEaseOut(timer, 0f, 1f, appearanceInterval);
            // alertRect.localScale = sizeDelta;
            textMesh.transform.localScale = sizeDelta;
            yield return null;
        }
        while (timer < 2.5f) {
            timer += Time.deltaTime;
            yield return null;
        }
        textMesh.transform.localScale = Vector2.one;
        // alertIcon.enabled = false;
        // textMesh.transform.localScale =
        textMesh.enabled = false;
    }


}
