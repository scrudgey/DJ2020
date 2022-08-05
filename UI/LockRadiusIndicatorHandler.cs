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
            SetScale();
        } else {
            DisableCursorImage();
        }
    }
    void DisableCursorImage() {
        cursorImage.enabled = false;
    }
    public void SetScale() {
        float locksizeInPixels = 1f;
        if (UICamera.orthographic) {
            float lengthPerAngle = (UICamera.orthographicSize * 2) / (UICamera.fieldOfView); // ?
            float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
            locksizeInPixels = (2f / lengthPerAngle) * (pixelsPerDegree);
        }
        cursorRect.sizeDelta = Vector2.one * locksizeInPixels * 1.02f;
        cursorMaskRect.sizeDelta = Vector2.one * locksizeInPixels;
    }
}
