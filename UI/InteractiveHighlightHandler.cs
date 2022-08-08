using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveHighlightHandler : IBinder<Interactor> {
    // Interactor IBinder<Interactor>.target { get; set; }

    public Camera cam;
    public RectTransform cursor;
    public HighlightableTargetData data;
    public TextMeshProUGUI cursorText;
    public TextMeshProUGUI dotText;
    Coroutine blitTextCoroutine;
    public AudioSource audioSource;
    public Image cursorImage;
    private float timer;
    public Color color;
    void Awake() {
        blitTextCoroutine = null;
    }
    override public void HandleValueChanged(Interactor interactor) {
        HighlightableTargetData newData = interactor.highlighted;
        InteractorTargetData activeTarget = interactor.ActiveTarget();
        if (newData == null && activeTarget == null) {
            Disable();
            data = null;
        } else if (activeTarget != null) {
            if (!InteractorTargetData.Equality(data, activeTarget)) {
                data?.target?.DisableOutline();
                data = activeTarget;
                DataChanged();
            }
        } else if (newData != null) {
            if (!InteractorTargetData.Equality(data, newData)) {
                data?.target?.DisableOutline();
                data = newData;
                DataChanged();
            }
        }
    }
    void DataChanged() {
        if (data == null) {
            Disable();
        } else if (data.target != null) {
            Enable(data.target.calloutText);
        }
    }

    void Update() {
        timer += Time.deltaTime;
        if (data == null) {
            Disable();
            timer = 0f;
        } else if (data?.target == null) {
            data = null;
            timer = 0f;
            DataChanged();
            return;
        } else if (data != null) {
            Vector3 screenPoint = cam.WorldToScreenPoint(data.collider.bounds.center);
            cursor.position = screenPoint;
            cursorText.color = color;
            dotText.color = color;
            cursorImage.color = color;
            SetScale();
            cursorImage.enabled = false;
        }
    }
    void Disable() {
        dotText.enabled = false;
        cursorText.enabled = false;
        cursorImage.enabled = false;
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
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        // if (data != null) {
        data.target.EnableOutline();
        cursorText.enabled = true;

        cursorImage.enabled = true;
        cursorText.text = "";
        dotText.enabled = true;
        blitTextCoroutine = StartCoroutine(BlitCalloutText(actionText));
        // }
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
    public void SetScale() {
        float distance = Vector3.Distance(cam.transform.position, data.target.transform.position);
        float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float inaccuracyLength = data.collider.bounds.size.magnitude / 2f;
        float pixelsPerLength = cam.scaledPixelHeight / frustumHeight;
        float pixelScale = 2f * inaccuracyLength * pixelsPerLength;

        // float dynamicCoefficient = 1.0f + 0.05f * Mathf.Sin(timer);
        // float dynamicCoefficient = Toolbox.Triangle(0.95f, 1.05f, 5f, 0f, timer);
        float dynamicCoefficient = 1f;

        cursor.sizeDelta = dynamicCoefficient * pixelScale * Vector2.one;
    }
}
