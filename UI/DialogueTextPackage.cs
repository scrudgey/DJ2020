using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
// WTH is up with this name? this is the only object named like this. call it a controller or handler please.
public class DialogueTextPackage : MonoBehaviour {
    public RectTransform container;
    public TextMeshProUGUI text;
    public GameObject leftPadding;
    public GameObject rightPadding;
    public void Initialize(string content, bool left) {
        text.text = content;
        leftPadding.SetActive(!left);
        rightPadding.SetActive(left);
    }
    public void Remove() {
        StartCoroutine(easeOut());
    }
    IEnumerator easeOut() {
        float fullHeight = container.rect.height;
        float width = container.rect.width;
        float timer = 0f;
        float duration = 1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float height = (float)PennerDoubleAnimation.ExpoEaseOut(timer, fullHeight, -fullHeight, duration);
            container.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        Destroy(gameObject);
    }
}
