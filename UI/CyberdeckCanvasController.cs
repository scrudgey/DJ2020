using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CyberdeckCanvasController : MonoBehaviour {
    public TextMeshProUGUI mainText;
    [Header("bodies")]
    public GameObject bodyText;
    public GameObject bodyDetect;
    public GameObject bodyMenu;
    void Start() {
        bodyText.SetActive(true);
        bodyDetect.SetActive(false);
        bodyMenu.SetActive(false);
        StartCoroutine(AnimateMainText());
    }

    public void HandleConnection(bool connected) {
        if (connected) {
            bodyText.SetActive(false);
            bodyDetect.SetActive(true);
            bodyMenu.SetActive(false);
            StartCoroutine(BlinkDetect(ShowBodyMenu));
        } else {
            bodyText.SetActive(true);
            bodyDetect.SetActive(false);
            bodyMenu.SetActive(false);
        }
    }

    void ShowBodyMenu() {
        bodyText.SetActive(false);
        bodyDetect.SetActive(false);
        bodyMenu.SetActive(true);
    }

    IEnumerator BlinkDetect(Action callback) {
        float timer = 0f;
        float duration = 0.5f;
        float blinkTimer = 0f;
        float blinkInterval = 0.03f;
        float hangtime = 1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            blinkTimer += Time.unscaledDeltaTime;
            if (blinkTimer > blinkInterval) {
                blinkTimer -= blinkInterval;
                bodyDetect.SetActive(!bodyDetect.activeInHierarchy);
            }
            yield return null;
        }
        bodyDetect.SetActive(true);
        timer = 0f;
        while (timer < hangtime) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        callback?.Invoke();
    }
    IEnumerator AnimateMainText() {
        float timer = 0f;
        float duration = 0.5f;
        int index = 0;
        while (true) {
            timer += Time.unscaledDeltaTime;
            if (timer > duration) {
                timer -= duration;
                index += 1;
                if (index > 3) index = 0;
                mainText.text = index switch {
                    0 => "Scanning",
                    1 => "Scanning.",
                    2 => "Scanning..",
                    3 => "Scanning...",
                    _ => "Scanning"
                };
            }
            yield return null;
        }
    }
}
