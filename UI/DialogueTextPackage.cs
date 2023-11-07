using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
// WTH is up with this name? this is the only object named like this. call it a controller or handler please.
public class DialogueTextPackage : MonoBehaviour {
    public RectTransform container;
    public TextMeshProUGUI text;
    public TextMeshProUGUI subtext;
    public GameObject leftPadding;
    public GameObject rightPadding;
    public IEnumerator Initialize(string content, string subContent, bool left) {
        // text.text = content;
        subtext.text = subContent;
        leftPadding.SetActive(!left);
        rightPadding.SetActive(left);
        // if (left) {
        //     text.alignment = TextAlignmentOptions.Left;
        // } else {
        //     text.alignment = TextAlignmentOptions.Right;
        // }
        return Toolbox.BlitText(text, content, interval: 0.02f);
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
