using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

public class AlertHandler : MonoBehaviour {
    public RectTransform alertRect;
    public SpriteRenderer alertIcon;
    public Material alertMaterial;
    public Material warnMaterial;
    private Coroutine coroutine;

    void Awake() {
        alertIcon.enabled = false;
    }
    public void Hide() {
        alertIcon.enabled = false;
    }

    public void ShowAlert() {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        alertIcon.material = alertMaterial;
        StartCoroutine(ShowAlertIcon());
    }
    public void ShowWarn() {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        alertIcon.material = warnMaterial;
        StartCoroutine(ShowAlertIcon());
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

}
