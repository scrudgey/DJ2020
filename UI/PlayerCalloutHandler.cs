using System.Collections;
using System.Collections.Generic;
using Easings;

using UnityEngine;
public class PlayerCalloutHandler : MonoBehaviour {
    public CutoutMaskUI cursorImage;
    public RectTransform cursorRect;
    public RectTransform cursorMaskRect;
    public Camera UICamera;
    public AudioSource audioSource;
    public AudioClip activateSound;
    Color initialColor;
    float transitionTime;
    float transitionDuration = 0.5f;
    float scaleFactor = 1f;
    float alpha = 1f;
    float effectScale = 0.45f;
    void Start() {
        initialColor = cursorImage.color;
    }
    public void ActivatePlayerCallout() {
        cursorImage.enabled = true;
        transitionTime = 0f;
        scaleFactor = 1f + effectScale;
        alpha = 0f;
        audioSource.PlayOneShot(activateSound);
        cursorMaskRect.position = data.screenPosition;
        SetScale();
    }
    void DisableCursorImage() {
        cursorImage.enabled = false;
    }
    void Update() {
        if (cursorImage.enabled) {
            transitionTime += Time.unscaledDeltaTime;
            if (transitionTime > transitionDuration) {
                scaleFactor = 1f;
            } else {
                scaleFactor = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 1f + effectScale, -1f * effectScale, transitionDuration);
                alpha = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 0f, initialColor.a, transitionDuration);
            }
        }
    }
    public void SetScale() {
        // TODO: locksize depends on gun
        float locksizeInPixels = gunHandler.gunInstance.baseGun.lockOnSize;
        if (UICamera.orthographic) {
            float lengthPerAngle = (UICamera.orthographicSize * 2) / (UICamera.fieldOfView); // ?
            float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
            locksizeInPixels = (locksizeInPixels / lengthPerAngle) * (pixelsPerDegree);
        }
        cursorRect.sizeDelta = Vector2.one * locksizeInPixels * 1.02f * scaleFactor;
        cursorMaskRect.sizeDelta = Vector2.one * locksizeInPixels * scaleFactor;
        cursorImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
    }
}
