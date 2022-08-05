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
            CursorData data = target.currentTargetData;
            if (data == null)
                return;
            cursorMaskRect.position = data.screenPosition;
            SetScale();
        } else {
            cursorImage.enabled = false;
        }
    }
    public void SetScale() {
        float locksizeInPixels = 1f;
        if (UICamera.orthographic) {
            // length per angle: (UICamera.orthographicSize * 2?) / (UICamera.fieldOfView) ???
            // pixels per degree: (UICamera.scaledPixelHeight / UICamera.fieldOfView)
            // (inaccuracy / length per angle) * (pixels per degree) = inaccuracy in pixels
            float lengthPerAngle = (UICamera.orthographicSize * 2) / (UICamera.fieldOfView); // ?
            float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
            locksizeInPixels = (2f / lengthPerAngle) * (pixelsPerDegree);
        }
        //else {
        //     float distance = Vector3.Distance(UICamera.transform.position, target.transform.position);
        //     float inaccuracyDegree = Mathf.Atan(inaccuracyLength / distance) * Mathf.Rad2Deg;
        //     float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
        //     inaccuracyInPixels = inaccuracyDegree * pixelsPerDegree / 2f;
        // }
        // cursorRect.sizeDelta = Vector2.one * locksizeInPixels;
        cursorRect.sizeDelta = Vector2.one * locksizeInPixels * 1.02f;
        cursorMaskRect.sizeDelta = Vector2.one * locksizeInPixels;
    }
}
