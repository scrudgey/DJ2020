using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LockRadiusIndicatorHandler : IBinder<GunHandler> {
    // public Image cursorImage;
    public CutoutMaskUI cursorImage;
    public RectTransform cursorRect;
    public RectTransform cursorMaskRect;
    public Camera UICamera;

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
            cursorMaskRect.position = data.screenPosition;
            SetScale(gunHandler);
        } else {
            DisableCursorImage();
        }
    }
    void DisableCursorImage() {
        cursorImage.enabled = false;
    }
    public void SetScale(GunHandler gunHandler) {
        // TODO: locksize depends on gun
        float locksizeInPixels = gunHandler.gunInstance.baseGun.lockOnSize;
        if (UICamera.orthographic) {
            float lengthPerAngle = (UICamera.orthographicSize * 2) / (UICamera.fieldOfView); // ?
            float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
            locksizeInPixels = (locksizeInPixels / lengthPerAngle) * (pixelsPerDegree);
        }
        cursorRect.sizeDelta = Vector2.one * locksizeInPixels * 1.02f;
        cursorMaskRect.sizeDelta = Vector2.one * locksizeInPixels;
    }
}
