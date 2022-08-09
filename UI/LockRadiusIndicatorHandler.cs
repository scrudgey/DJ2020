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
    public EmbellishDotText embellishDotText;
    public RectTransform embellishRectTransform;
    Color initialColor;
    bool hasGun;
    float transitionTime;
    float transitionDuration = 0.5f;
    float scaleFactor = 1f;
    float alpha = 1f;
    float effectScale = 0.45f;
    float currentScale;
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
                // DisableCursorImage();
                cursorMaskRect.position = data.mousePosition;
                cursorImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a / 6f);
                return;
            }
            if (!hasGun) {
                transitionTime = 0f;
                scaleFactor = 1f + effectScale;
                alpha = 0f;
                audioSource.PlayOneShot(activateSound);
            }
            SetScale(gunHandler, data);
            hasGun = true;
        } else {
            DisableCursorImage();
            hasGun = false;
        }
    }
    void DisableCursorImage() {
        cursorImage.enabled = false;
        embellishDotText.Disable();
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
    public void SetScale(GunHandler gunHandler, CursorData data) {
        cursorMaskRect.position = data.screenPosition;
        float locksizeInPixels = gunHandler.gunInstance.baseGun.lockOnSize;
        float lengthPerAngle = (UICamera.orthographicSize * 2) / (UICamera.fieldOfView); // ?
        float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
        locksizeInPixels = (locksizeInPixels / lengthPerAngle) * (pixelsPerDegree);
        // pix          :       m   / (m / θ)           * (pix / θ)
        cursorImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

        float targetScale = locksizeInPixels * scaleFactor;
        currentScale = Mathf.Lerp(currentScale, targetScale, 0.1f);
        cursorRect.sizeDelta = Vector2.one * currentScale * 1.02f;
        cursorMaskRect.sizeDelta = Vector2.one * currentScale;
        embellishRectTransform.position = data.screenPosition + currentScale * new Vector2(0.25f, -0.55f);
        embellishDotText.Enable($"Lock: {currentScale / pixelsPerDegree * lengthPerAngle:0.00}m", blitText: false);
        //                                  pix       /     (pix / θ)   *    (m / θ)

    }
}
