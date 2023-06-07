using System.Collections;
using System.Collections.Generic;
using Easings;
using Easings;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressPip : MonoBehaviour {
    public Image fillPip;
    public Color baseColor;
    Coroutine pulseRoutine;
    public RectTransform pulserImage;
    public bool complete;
    void Start() {
        pulserImage.gameObject.SetActive(false);
    }
    public void SetProgress(bool inProgress, bool complete) {
        if (complete) {
            SetComplete();
        } else if (inProgress) {
            SetInProgress();
        } else {
            SetIncomplete();
        }
    }
    public void ShowPulse() {
        StartCoroutine(PulseIndicator());
    }

    void SetComplete() {
        if (pulseRoutine != null) {
            StopCoroutine(pulseRoutine);
        }
        fillPip.color = baseColor;
        pulseRoutine = null;
        if (!complete) {
            ShowPulse();
        }
        complete = true;
    }

    void SetInProgress() {
        complete = false;
        if (pulseRoutine != null) return;
        pulseRoutine = StartCoroutine(PulseColor());
    }

    void SetIncomplete() {
        complete = false;
        if (pulseRoutine != null) {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
        fillPip.color = Color.black;
        pulseRoutine = null;
    }

    IEnumerator PulseColor() {
        float timer = 0f;
        float duration = 0.25f;
        while (true) {
            timer += Time.unscaledDeltaTime;
            float alpha = (float)PennerDoubleAnimation.ExpoEaseInOut(timer, 0.2f, 0.6f, duration);
            Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            Debug.Log(newColor);
            fillPip.color = newColor;
            if (timer > duration) {
                timer -= duration;
            }
            yield return null;
        }
    }

    IEnumerator PulseIndicator() {
        float timer = 0f;
        float duration = 0.5f;
        pulserImage.gameObject.SetActive(true);
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float scale = (float)PennerDoubleAnimation.Linear(timer, 25f, 75f, duration);
            pulserImage.sizeDelta = scale * Vector3.one;
            yield return null;
        }
        pulserImage.gameObject.SetActive(false);
    }
}
