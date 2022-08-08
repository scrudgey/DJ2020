using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class LockRadiusIndicatorHandler : IBinder<GunHandler> {
    // public Image cursorImage;
    public CutoutMaskUI cursorImage;
    public RectTransform cursorRect;
    public RectTransform cursorMaskRect;
    public Camera UICamera;
    public AudioSource audioSource;
    public AudioClip activateSound;
    Color initialColor;
    bool hasGun;
    float transitionTime;
    float transitionDuration = 0.5f;
    float scaleFactor = 1f;
    float alpha = 1f;
    float effectScale = 0.45f;
    void Start() {
        initialColor = cursorImage.color;
    }
    override public void HandleValueChanged(GunHandler gunHandler) {
        if (gunHandler.HasGun()) {
            cursorImage.enabled = true;
            CursorData data = gunHandler.currentTargetData;
            if (data == null) {
                DisableCursorImage();
                return;
            }
            if (data.type == CursorData.TargetType.objectLock) {
                DisableCursorImage();
                return;
            }
            if (!hasGun) {
                transitionTime = 0f;
                scaleFactor = 1f + effectScale;
                alpha = 0f;
                audioSource.PlayOneShot(activateSound);
            }
            cursorMaskRect.position = data.screenPosition;
            SetScale(gunHandler);
            hasGun = true;
        } else {
            DisableCursorImage();
            hasGun = false;
        }
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
    public void SetScale(GunHandler gunHandler) {
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
