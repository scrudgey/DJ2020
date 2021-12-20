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
            cursorImage.color = Color.green;
            cursorText.color = Color.green;
            dotText.color = Color.green;
            SetScale();
        }

    }
    void Disable() {
        dotText.enabled = false;
        cursorImage.enabled = false;
        cursorText.enabled = false;
        cursorText.text = "";
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        if (data != null) {
            data.target.DisableOutline();
        }
    }
    void Enable(string actionText) {
        if (data != null) {
            cursorImage.enabled = true;
            cursorText.enabled = true;
            data.target.EnableOutline();

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
    public void SetScale() {
        float distance = Vector3.Distance(cam.transform.position, data.target.transform.position);
        float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float inaccuracyLength = data.collider.bounds.size.magnitude / 2f;
        float pixelsPerLength = cam.scaledPixelHeight / frustumHeight;
        float pixelScale = 2f * inaccuracyLength * pixelsPerLength;

        cursor.sizeDelta = pixelScale * Vector2.one;
    }
}
