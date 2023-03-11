using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class SaveIndicatorController : MonoBehaviour {
    public Image icon;
    Coroutine saveIndicatorCoroutine;

    public void HideIndicator() {
        icon.enabled = false;
    }
    public void ShowSaveIndicator() {
        if (saveIndicatorCoroutine != null) {
            StopCoroutine(saveIndicatorCoroutine);
        }
        saveIndicatorCoroutine = StartCoroutine(SaveIndicatorShow());
    }
    IEnumerator SaveIndicatorShow() {
        float timer = 0f;
        float duration = 1f;
        icon.enabled = true;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float alpha = (float)PennerDoubleAnimation.CircEaseInOut(timer, 0f, 1f, duration);
            Color color = Color.white;
            color.a = alpha;
            icon.color = color;
            yield return null;
        }
        icon.enabled = false;
        saveIndicatorCoroutine = null;
    }
}
