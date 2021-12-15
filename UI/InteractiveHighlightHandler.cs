using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveHighlightHandler : MonoBehaviour, IBinder<Interactor> {
    Interactor IBinder<Interactor>.target { get; set; }

    public Camera cam;
    public RectTransform cursor;
    public InteractorTargetData data;
    public TextMeshProUGUI cursorText;
    public TextMeshProUGUI dotText;
    Coroutine blitTextCoroutine;

    void Awake() {
        blitTextCoroutine = null;
    }
    public void HandleValueChanged(Interactor interactor) {
        InteractorTargetData newData = interactor.highlighted;
        if (!InteractorTargetData.Equality(data, newData)) {
            if (data != null && data.target != null)
                data.target.DisableOutline();
            data = newData;
            DataChanged();
        }
        if (data == null) {
            cursorText.enabled = false;
        } else {
            if (data.target != null)
                data.target.EnableOutline();
            cursorText.enabled = true;
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
        cursorText.text = "";
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        if (data != null) {
            data.target.DisableOutline();
        }
        dotText.enabled = false;
    }
    void Enable(string actionText) {
        cursorText.text = "";
        dotText.enabled = true;
        blitTextCoroutine = StartCoroutine(BlitCalloutText(actionText));
        if (data != null) {
            data.target.EnableOutline();
        }
    }
    public IEnumerator BlitCalloutText(string actionText) {
        float blitInterval = 0.02f;
        float timer = 0f;
        int index = 1;
        dotText.enabled = true;
        string targetText = $"{actionText}";
        while (cursorText.text != targetText) {
            while (timer < blitInterval) {
                timer += Time.deltaTime;
                yield return null;
            }
            timer -= blitInterval;
            index += 1;
            cursorText.text = targetText.Substring(0, index);
        }
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
