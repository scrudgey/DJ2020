using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapMarkerIndicator : MonoBehaviour {
    public RectTransform rectTransform;
    public TextMeshProUGUI text;
    public Image icon;

    [Header("Icons")]
    public Sprite circleSprite;
    public Sprite lightningBoltSprite;
    public Sprite arrowSprite;
    public Sprite doorSprite;
    public Sprite cameraSprite;
    public Image clickPulseIndicator;
    public Image selectedIndicator;
    [Header("Colors")]
    public Color green;
    public Color red;
    public Color blue;
    public Color yellow;
    Coroutine pulseCoroutine;
    Coroutine selectionCoroutine;
    void Start() {
        clickPulseIndicator.enabled = false;
        selectedIndicator.enabled = false;
    }

    public void Configure(MapMarkerData data) {
        Configure(data.markerName, data.markerType, data.markerIcon);
    }
    public void Configure(string markerName, MapMarkerData.MapMarkerType markerType, MapMarkerData.MapMarkerIcon markerIcon) {
        text.text = markerName;

        switch (markerType) {
            case MapMarkerData.MapMarkerType.decor:
                text.color = blue;
                icon.color = blue;
                break;
            case MapMarkerData.MapMarkerType.extractionPoint:
                text.color = yellow;
                icon.color = yellow;
                break;
            case MapMarkerData.MapMarkerType.guard:
                text.color = red;
                icon.color = red;
                break;
            case MapMarkerData.MapMarkerType.insertionPoint:
                text.color = green;
                icon.color = green;
                break;
            case MapMarkerData.MapMarkerType.objective:
                text.color = red;
                icon.color = red;
                break;
            case MapMarkerData.MapMarkerType.pointOfInterest:
                text.color = blue;
                icon.color = blue;
                break;
        }

        icon.sprite = markerIcon switch {
            MapMarkerData.MapMarkerIcon.arrowDown => arrowSprite,
            MapMarkerData.MapMarkerIcon.arrowLeft => arrowSprite,
            MapMarkerData.MapMarkerIcon.arrowRight => arrowSprite,
            MapMarkerData.MapMarkerIcon.arrowUp => arrowSprite,
            MapMarkerData.MapMarkerIcon.circle => circleSprite,
            MapMarkerData.MapMarkerIcon.lightningBolt => lightningBoltSprite,
            MapMarkerData.MapMarkerIcon.door => doorSprite,
            MapMarkerData.MapMarkerIcon.camera => cameraSprite,
            _ => circleSprite
        };

        if (markerIcon == MapMarkerData.MapMarkerIcon.arrowLeft) {
            icon.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        } else if (markerIcon == MapMarkerData.MapMarkerIcon.arrowDown) {
            icon.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        } else if (markerIcon == MapMarkerData.MapMarkerIcon.arrowRight) {
            icon.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
        }
    }

    public void SetPosition(MapMarkerData data, RectTransform parentRect) {
        rectTransform.anchoredPosition = data.position * parentRect.rect.width;
    }
    public void SetPosition(Vector3 worldPosition, MapDisplay3DGenerator mapDisplay3Dgenerator, RectTransform parentRect) {
        Vector3 viewPosition = mapDisplay3Dgenerator.WorldToViewportPoint(worldPosition);
        rectTransform.anchoredPosition = viewPosition * parentRect.rect.width;
    }

    public void ShowClickPulse() {
        if (pulseCoroutine == null) {
            pulseCoroutine = StartCoroutine(Pulse());
        }
    }
    public void ShowSelection(bool value) {
        if (selectionCoroutine != null) {
            StopCoroutine(selectionCoroutine);
        }
        if (value) {
            if (gameObject.activeInHierarchy)
                selectionCoroutine = StartCoroutine(ShowSelection());
        } else {
            // StopCoroutine(selectionCoroutine);
            selectedIndicator.enabled = false;
        }
    }

    IEnumerator Pulse() {
        float timer = 0f;
        float duration = 0.5f;
        clickPulseIndicator.enabled = true;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float factor = (float)PennerDoubleAnimation.Linear(timer, 1f, 10f, duration);
            clickPulseIndicator.transform.localScale = factor * Vector3.one;
            clickPulseIndicator.enabled = !clickPulseIndicator.enabled;
            yield return null;
        }
        pulseCoroutine = null;
        clickPulseIndicator.enabled = false;
    }

    IEnumerator ShowSelection() {
        while (true) {
            selectedIndicator.enabled = !selectedIndicator.enabled;
            yield return null;
        }
    }

}
