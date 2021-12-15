using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI {
    public class AimIndicatorHandler : MonoBehaviour, IBinder<GunHandler>, IBinder<NeoCharacterController> {

        private GunHandler gunHandlerTarget;
        private NeoCharacterController neoCharacterControllerTarget;
        GunHandler IBinder<GunHandler>.target {
            get { return gunHandlerTarget; }
            set { gunHandlerTarget = value; }
        }

        NeoCharacterController IBinder<NeoCharacterController>.target {
            get { return neoCharacterControllerTarget; }
            set { neoCharacterControllerTarget = value; }
        }


        public Camera UICamera;
        private NeoCharacterController targetCharacterController;
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
        void IBinder<GunHandler>.HandleValueChanged(GunHandler gunHandler) {
            if (gunHandler.HasGun()) {
                cursorImage.enabled = true;

                TargetData data = gunHandler.lastTargetData;
                if (data == null)
                    return;
                cursor.position = data.screenPosition;
                switch (data.type) {
                    case TargetData.TargetType.none:
                        break;
                    default:
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
            } else {
                cursorImage.enabled = false;
            }

        }
        void IBinder<NeoCharacterController>.HandleValueChanged(NeoCharacterController t) {
            if (neoCharacterControllerTarget.state == CharacterState.wallPress) {
                cursorImage.enabled = false;
            } else {
                cursorImage.enabled = true;
            }
        }
        public void SetScale() {
            float distance = Vector3.Distance(UICamera.transform.position, gunHandlerTarget.transform.position);
            float frustumHeight = 2.0f * distance * Mathf.Tan(UICamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            float inaccuracyLength = gunHandlerTarget.inaccuracy();
            float pixelsPerLength = UICamera.scaledPixelHeight / frustumHeight;
            float pixelScale = 2f * inaccuracyLength * pixelsPerLength;
            pixelScale = Mathf.Max(10, pixelScale) + pulseSize;

            cursor.sizeDelta = pixelScale * Vector2.one;
        }
    }

}