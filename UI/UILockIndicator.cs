using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class UILockIndicator : MonoBehaviour {
    public RectTransform rectTransform;
    public Image image;
    public Color color;
    public float duration = 1f;
    public float extent = 50f;
    void Start() {
        StartCoroutine(RiseAndFade());
    }

    IEnumerator RiseAndFade() {
        float timer = 0f;
        Vector3 initialPosition = rectTransform.anchoredPosition;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float factor = (float)PennerDoubleAnimation.Linear(timer, 0f, 1f, duration);
            Vector2 newPosition = new Vector2(initialPosition.x, initialPosition.y + (factor * extent));
            Color newColor = new Color(color.r, color.g, color.b, 1 - factor);
            image.color = newColor;
            rectTransform.anchoredPosition = newPosition;
            yield return null;
        }
        Destroy(gameObject);
    }
}
