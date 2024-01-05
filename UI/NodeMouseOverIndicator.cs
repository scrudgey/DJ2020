using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeMouseOverIndicator : MonoBehaviour {
    public RectTransform rectTransform;
    public Image image;
    public Color color;
    Coroutine routine;
    RectTransform followRect;
    public void ActivateSelection<T, U>(NodeIndicator<T, U> indicator) where T : Node<T> where U : Graph<T, U> {
        gameObject.SetActive(true);
        image.color = color;
        rectTransform.anchoredPosition = indicator.rectTransform.anchoredPosition;
        if (routine != null) {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(blinkSelection());
        followRect = indicator.rectTransform;
    }
    void Update() {
        if (followRect != null)
            rectTransform.anchoredPosition = followRect.anchoredPosition;
    }
    public void HideSelection() {
        gameObject.SetActive(false);
        followRect = null;
    }

    IEnumerator blinkSelection() {
        rectTransform.sizeDelta = 90f * Vector2.one;
        return Toolbox.BlinkEmphasis(image, unlimited: true);
    }
}
