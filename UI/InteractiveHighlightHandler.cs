using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveHighlightHandler : MonoBehaviour, IBinder<Interactor> {
    Interactor IBinder<Interactor>.target { get; set; }

    public Camera cam;
    public RectTransform cursor;
    public HighlightableTargetData data;
    public TextMeshProUGUI cursorText;
    public TextMeshProUGUI dotText;
    Coroutine blitTextCoroutine;
    public AudioSource audioSource;

    void Awake() {
        blitTextCoroutine = null;
    }
    public void HandleValueChanged(Interactor interactor) {
        HighlightableTargetData newData = interactor.highlighted;
        if (!InteractorTargetData.Equality(data, newData)) {
            if (data != null && data.target != null)
                Disable();
            // data.target.DisableOutline();
            data = newData;
            DataChanged();
        }
    }
    void DataChanged() {
        if (data == null) {
            Disable();
        } else {
            Enable(data.target.calloutText);
        }
    }

    void Update() {
        if (data == null) {
            Disable();
        } else if (data.target == null) {
            data = null;
            DataChanged();
            return;
        } else if (data != null) {
            Vector3 screenPoint = cam.WorldToScreenPoint(data.collider.bounds.center);
            cursor.position = screenPoint;
            cursorText.color = Color.green;
            dotText.color = Color.green;
        }
    }
    void Disable() {
        dotText.enabled = false;
        cursorText.enabled = false;
        cursorText.text = "";
        audioSource.Stop();
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        if (data != null) {
            data.target.DisableOutline();
        }
    }
    void Enable(string actionText) {
        if (data != null) {
            data.target.EnableOutline();
            cursorText.enabled = true;

            cursorText.text = "";
            dotText.enabled = true;
            blitTextCoroutine = StartCoroutine(BlitCalloutText(actionText));
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
