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
            if (gunHandler.HasGun() && gunHandler.inputMode == InputMode.gun) {
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
                // TODO: handle this?
                // SetScale();
            } else {
                cursorImage.enabled = false;
            }
        }

        public void SetScale() {
            float distance = Vector3.Distance(UICamera.transform.position, target.transform.position);
            float frustumHeight = 2.0f * distance * Mathf.Tan(UICamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            float inaccuracyLength = target.inaccuracy(target.currentTargetData);
            float pixelsPerLength = UICamera.scaledPixelHeight / frustumHeight;
            float pixelScale = 2f * inaccuracyLength * pixelsPerLength;
            pixelScale = Mathf.Max(10, pixelScale) + pulseSize;

            cursor.sizeDelta = pixelScale * Vector2.one;
        }
    }

}