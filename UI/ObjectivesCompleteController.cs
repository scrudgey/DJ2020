using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ObjectivesCompleteController : MonoBehaviour {
    public Canvas canvas;
    public TextMeshProUGUI text;
    public RectTransform textRect;
    Coroutine messageRoutine;
    public void Initialize() {
        canvas.enabled = false;
    }
    public void DisplayMessage(params string[] messages) {
        if (messageRoutine != null) {
            StopCoroutine(messageRoutine);
        }
        // foreach
        messageRoutine = StartCoroutine(Toolbox.ChainCoroutines(messages.ToList().Select(message => EaseInMessage(message)).ToArray()));
        // Toolbox.ChainCoroutines()
        // messageRoutine = StartCoroutine(EaseInMessage(message));
    }

    IEnumerator EaseInMessage(string message) {
        float timer = 0f;
        float duration = 1f;
        text.text = message;

        float initialPosition = (1920f / 2f) + textRect.rect.width / 2f;
        textRect.anchoredPosition = new Vector2(initialPosition, 0f);

        canvas.enabled = true;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float x = (float)PennerDoubleAnimation.ExpoEaseOut(timer, initialPosition, -1f * initialPosition, duration);
            textRect.anchoredPosition = new Vector2(x, 0f);
            yield return null;
        }
        timer = 0f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        timer = 0f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float x = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, -1f * initialPosition, duration);
            textRect.anchoredPosition = new Vector2(x, 0f);
            yield return null;
        }
        canvas.enabled = false;
        messageRoutine = null;
    }
}
