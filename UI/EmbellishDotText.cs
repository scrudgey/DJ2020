using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmbellishDotText : MonoBehaviour {
    public Camera cam;
    public TextMeshProUGUI cursorText;
    public TextMeshProUGUI dotText;
    Coroutine blitTextCoroutine;
    public AudioSource audioSource;
    private float timer;
    public Color color;

    void Update() {
        timer += Time.deltaTime;
        cursorText.color = color;
        dotText.color = color;
    }
    public void Disable() {
        dotText.enabled = false;
        cursorText.enabled = false;
        cursorText.text = "";
        audioSource.Stop();
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
    }
    public void Enable(string actionText, bool blitText = true) {
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        cursorText.enabled = true;
        cursorText.text = "";
        dotText.enabled = true;
        if (blitText) {
            blitTextCoroutine = StartCoroutine(BlitCalloutText(actionText));
        } else {
            cursorText.text = actionText;
        }
    }
    public IEnumerator BlitCalloutText(string actionText) {
        float blitInterval = 0.02f;
        float timer = 0f;
        int index = 1;
        dotText.enabled = true;
        string targetText = $"{actionText}";
        audioSource.Play();
        while (cursorText.text != targetText) {
            while (timer < blitInterval) {
                timer += Time.deltaTime;
                yield return null;
            }
            timer -= blitInterval;
            index += 1;
            cursorText.text = targetText.Substring(0, index);
        }
        audioSource.Stop();
        timer = 0f;
        blitInterval = 0.5f;
        while (true) {
            while (timer < blitInterval) {
                timer += Time.deltaTime;
                yield return null;
            }
            timer -= blitInterval;
            dotText.enabled = !dotText.enabled;
            yield return null;
        }
    }
}
