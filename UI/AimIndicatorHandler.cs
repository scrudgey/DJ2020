using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI {
    public class AimIndicatorHandler : MonoBehaviour {
        public Camera UICamera;
        private GunHandler target;
        public RectTransform cursor;
        public Image cursorImage;
        public Sprite directionAimSprite;
        public Sprite objectLockSprite;
        public Color directionAimColor;
        public Color objectLockColor;
        private float timer;
        public float pulseInterval = 0.15f;
        public TargetData.TargetType state;
        public int pulseSize;
        public void Bind(GameObject newTargetObject) {
            if (target != null) {
                target.OnTargetChanged -= HandleValueChanged;
            }
            target = newTargetObject.GetComponentInChildren<GunHandler>();
            if (target != null) {
                target.OnTargetChanged += HandleValueChanged;
                HandleValueChanged(target);
            }
        }
        public void Update() {
            if (state == TargetData.TargetType.objectLock) {
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
        public void HandleValueChanged(GunHandler gunHandler) {
            TargetData data = gunHandler.lastTargetData;
            if (data == null)
                return;
            cursor.position = data.screenPosition;
            switch (data.type) {
                case TargetData.TargetType.none:
                    break;
                case TargetData.TargetType.direction:
                    cursorImage.sprite = directionAimSprite;
                    cursorImage.color = directionAimColor;
                    break;
                case TargetData.TargetType.objectLock:
                    cursorImage.sprite = objectLockSprite;
                    cursorImage.color = objectLockColor;
                    break;
            }
            state = data.type;
            SetScale();
        }
        public void SetScale() {
            float distance = Vector3.Distance(UICamera.transform.position, target.transform.position);
            float frustumHeight = 2.0f * distance * Mathf.Tan(UICamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            float inaccuracyLength = target.inaccuracy();
            float pixelsPerLength = UICamera.scaledPixelHeight / frustumHeight;
            float pixelScale = 2f * inaccuracyLength * pixelsPerLength;
            pixelScale = Mathf.Max(10, pixelScale) + pulseSize;

            cursor.sizeDelta = pixelScale * Vector2.one;
        }
    }

}