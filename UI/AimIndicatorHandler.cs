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
        private float timeInNormalAim;
        public float pulseInterval = 0.15f;
        public CursorData.TargetType state;
        public int pulseSize;
        Vector2 currentPosition;
        InputMode lastInputMode;

        public void Update() {
            if (state == CursorData.TargetType.objectLock) {
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
                CursorData data = target.currentTargetData;
                if (data == null)
                    return;
                Vector2 targetPosition = data.screenPosition;
                switch (data.type) {
                    case CursorData.TargetType.none:
                        timeInNormalAim = 0f;
                        break;
                    default:
                    case CursorData.TargetType.direction:
                        timeInNormalAim += Time.unscaledDeltaTime;
                        if (timeInNormalAim < 0.25f) {
                            targetPosition = Vector2.Lerp(currentPosition, targetPosition, 0.15f);
                        }
                        cursorImage.sprite = directionAimSprite;
                        cursorImage.color = directionAimColor;
                        break;
                    case CursorData.TargetType.objectLock:
                        timeInNormalAim = 0f;
                        cursorImage.sprite = objectLockSprite;
                        cursorImage.color = objectLockColor;
                        targetPosition = Vector2.Lerp(currentPosition, targetPosition, 0.15f);
                        break;
                }
                cursor.position = targetPosition;
                currentPosition = cursor.position;
                state = data.type;
                SetScale();
                lastInputMode = GameManager.I.inputMode;
            } else {
                cursorImage.enabled = false;
            }
        }

        public void SetScale() {
            float inaccuracyLength = target.inaccuracy(target.currentTargetData);
            float inaccuracyInPixels = 1f;
            if (UICamera.orthographic) {
                float lengthPerAngle = (UICamera.orthographicSize * 2) / (UICamera.fieldOfView); // ?
                float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
                inaccuracyInPixels = (inaccuracyLength / lengthPerAngle) * (pixelsPerDegree);

            } else {
                // TODO: fix this, should be fixed angular radius
                float distance = Vector3.Distance(UICamera.transform.position, target.transform.position);
                float inaccuracyDegree = Mathf.Atan(inaccuracyLength / distance) * Mathf.Rad2Deg;
                float pixelsPerDegree = (UICamera.scaledPixelHeight / UICamera.fieldOfView);
                inaccuracyInPixels = inaccuracyDegree * pixelsPerDegree / 2f;
            }
            inaccuracyInPixels = Mathf.Max(25, inaccuracyInPixels);
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