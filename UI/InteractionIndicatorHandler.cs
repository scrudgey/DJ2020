using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionIndicatorHandler : MonoBehaviour, IBinder<Interactor> {
    Interactor IBinder<Interactor>.target { get; set; }

    public Camera cam;
    public RectTransform cursor;
    public Image cursorImage;
    public InteractorTargetData data;
    public TextMeshProUGUI cursorText;
    public TextMeshProUGUI dotText;
    Coroutine blitTextCoroutine;

    // readonly static string dot = "<sprite index=0 tint>";

    public void HandleValueChanged(Interactor interactor) {
        InteractorTargetData newData = interactor.ActiveTarget();
        if (newData != data) {
            data = newData;
            DataChanged();
        }
        if (data == null) {
            cursorImage.enabled = false;
            cursorText.enabled = false;
        } else {
            cursorImage.enabled = true;
            cursorText.enabled = true;
        }
    }
    void DataChanged() {
        cursorText.text = "";
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        if (data == null)
            return;
        blitTextCoroutine = StartCoroutine(BlitCalloutText(data.target.calloutText));
    }

    void Update() {
        if (data == null)
            return;
        if (data.target == null) {
            data = null;
            DataChanged();
            return;
        }
        Vector3 screenPoint = cam.WorldToScreenPoint(data.collider.bounds.center);
        cursor.position = screenPoint;
        cursorImage.color = Color.green;
        cursorText.color = Color.green;
        dotText.color = Color.green;
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
            // if (cursorText.text == $"{dot}{actionText}") {
            //     cursorText.text = $" {actionText}";
            // } else {
            //     cursorText.text = $"{dot}{actionText}";
            // }
            yield return null;
        }
    }
}
