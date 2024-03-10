using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeSelectionIndicator : MonoBehaviour {
    public RectTransform rectTransform;
    public Color color;
    public Image image;
    Coroutine routine;
    WaitForSecondsRealtime waiter = new WaitForSecondsRealtime(0.2f);
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
        rectTransform.sizeDelta = new Vector2(150f, 150f);
        return Toolbox.ChainCoroutines(
            waiter,
            Toolbox.BlinkVis(image, () => rectTransform.sizeDelta = 120f * Vector2.one),
            waiter,
            Toolbox.BlinkVis(image, () => rectTransform.sizeDelta = 100f * Vector2.one),
            waiter,
            Toolbox.BlinkEmphasis(image)
        );
    }
}
