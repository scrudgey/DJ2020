using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeMouseOverIndicator : MonoBehaviour {
    public RectTransform rectTransform;
    public Image image;
    Coroutine routine;
    RectTransform followRect;
    public void ActivateSelection<T, U>(NodeIndicator<T, U> indicator) where T : Node<T> where U : Graph<T, U> {
        gameObject.SetActive(true);
        image.color = Color.white;
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
        rectTransform.sizeDelta = new Vector2(80f, 80f);
        return Toolbox.BlinkEmphasis(image, unlimited: true);
    }
}
