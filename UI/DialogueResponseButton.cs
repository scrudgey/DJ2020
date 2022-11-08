using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
public class DialogueResponseButton : MonoBehaviour {
    public string response;
    public TextMeshProUGUI text;
    public RectTransform myRectTransform;
    public Action<DialogueResponseButton> responseCallback;
    public void Initialize(Action<DialogueResponseButton> responseCallback, string response, float easeOffset) {
        this.responseCallback = responseCallback;
        this.response = response;
        text.text = response;
        StartCoroutine(waitAndThen(easeOffset, easeInSize()));
    }
    public void OnClick() {
        responseCallback(this);
    }
    public IEnumerator waitAndThen(float delay, IEnumerator wrapped) {
        float width = myRectTransform.rect.width;
        myRectTransform.sizeDelta = new Vector2(width, 2f);
        yield return new WaitForSecondsRealtime(delay);
        yield return StartCoroutine(wrapped);
    }
    IEnumerator easeInSize() {
        // float targetHeight = LayoutUtility.GetPreferredHeight(transform)
        yield return new WaitForEndOfFrame();
        float targetHeight = text.preferredHeight + 10f;
        float timer = 0f;
        float duration = 0.20f;
        float width = myRectTransform.rect.width;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float height = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 2f, targetHeight, duration);
            myRectTransform.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        myRectTransform.sizeDelta = new Vector2(width, targetHeight);
    }
}
