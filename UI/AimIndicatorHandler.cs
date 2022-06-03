using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI {
    public class AimIndicatorHandler : IBinder<GunHandler> {
        public Camera UICamera;
        private CharacterController targetCharacterController;
        public RectTransform cursor;
        public Image cursorImage;
        public Sprite directionAimSprite;
        public Sprite objectLockSprite;
        public Color directionAimColor;
        public Color objectLockColor;
        private float timer;
        public float pulseInterval = 0.15f;
        public TargetData2.TargetType state;
        public int pulseSize;

        public void Update() {
            if (state == TargetData2.TargetType.objectLock) {
                timer += Time.deltaTime;
                while (timer > pulseInterval) {
                    timer -= pulseInterval;
                    pulseSize += 2;
                    if (pulseSize > 6) {
                        pulseSize = 0;
                    }
                    SetScale();
                }
            } else {
                pulseSize = 0;
            }
        }
        override public void HandleValueChanged(GunHandler gunHandler) {
            if (gunHandler.HasGun()) {
                cursorImage.enabled = true;
                TargetData2 data = target.currentTargetData;
                if (data == null)
                    return;
                cursor.position = data.screenPosition;
                switch (data.type) {
                    case TargetData2.TargetType.none:
                        break;
                    default:
                    case TargetData2.TargetType.direction:
                        cursorImage.sprite = directionAimSprite;
                        cursorImage.color = directionAimColor;
                        break;
                    case TargetData2.TargetType.objectLock:
                        cursorImage.sprite = objectLockSprite;
                        cursorImage.color = objectLockColor;
                        break;
                }
                state = data.type;
                SetScale();
            } else {
                cursorImage.enabled = false;
            }
        }

        public void SetScale() {
            float inaccuracyLength = target.inaccuracy(target.currentTargetData);
            float inaccuracyInPixels = 1f;
            if (UICamera.orthographic) {
                // length per angle: (UICamera.orthographicSize * 2?) / (UICamera.fieldOfView) ???
                // pixels per degree: (UICamera.scaledPixelHeight / UICamera.fieldOfView)
                // (inaccuracy / length per angle) * (pixels per degree) = inaccuracy in pixels
                float lengthPerAngle = (UICamera.orthographicSize * 2) / (UICamera.fieldOfView); // ?
                float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
                inaccuracyInPixels = (inaccuracyLength / lengthPerAngle) * (pixelsPerDegree);

            } else {
                float distance = Vector3.Distance(UICamera.transform.position, target.transform.position);
                float inaccuracyDegree = Mathf.Atan(inaccuracyLength / distance) * Mathf.Rad2Deg;
                float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
                inaccuracyInPixels = inaccuracyDegree * pixelsPerDegree / 2f;
            }
            cursor.sizeDelta = inaccuracyInPixels * Vector2.one;
        }
    }
    //      l
    //  |-------
    //  |     /
    //  |    /
    //d |   / 
    //  |  /
    //  |θ/
    //  |/
    // 
    //  tan(θ) = l / d
    // 

    // FOV = width / height (degrees)

    // ultimately, we have:
    // inaccuracy is length
    // inaccuracy / (length per angle) -> inaccuracy in degrees
    // (inaccuracy in degrees) * (pixels per degree) -> inaccuracy in pixels
    // (inaccuracy / length per angle) * (pixels per degree) -> inaccuracy in pixels


    // UICamera.fieldOfView: vertical field of fiew in degrees
    // UICamera.orthographicSize: Camera's half-size when in orthographic mode. (length?)
    //      The orthographicSize is half the size of the vertical viewing volume. 
    // UICamera.scaledPixelHeight: How tall is the camera in pixels (accounting for dynamic resolution scaling) (Read Only)

    // length per angle: (UICamera.orthographicSize * 2?) / (UICamera.fieldOfView)
    // or squared or something?
    // pixels per degree: (UICamera.scaledPixelHeight / UICamera.fieldOfView)


    // (inaccuracy / length per angle) * (pixels per degree) -> inaccuracy in pixels
    // (inaccuracy / (UICamera.orthographicSize * 2?) / (UICamera.fieldOfView)) * (UICamera.scaledPixelHeight / UICamera.fieldOfView) -> inaccuracy in pixels

    // this is for something at the frustrum plane.

    // next, to establish a size at a distance:
    // tan(θ) = l / d
    // 
}