using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class AttackSurfaceDisplayCanvas : MonoBehaviour {
    public TextMeshProUGUI displayText;
    // public Image dataIcon;
    void Start() {
        StartCoroutine(BlinkTextCursor());
    }

    IEnumerator BlinkTextCursor() {
        float timer = 0f;
        float interval = 0.5f;
        while (true) {
            timer += Time.unscaledDeltaTime;
            if (timer > interval) {
                timer -= interval;
                ToggleCursor();
            }
            yield return null;
        }
    }

    void ToggleCursor() {
        string suffix = "<sprite=1>";
        if (displayText.text.EndsWith(suffix)) {
            displayText.text = displayText.text.Substring(0, displayText.text.Length - suffix.Length);
        } else {
            displayText.text = displayText.text + "<sprite=1>";
        }
    }
}
