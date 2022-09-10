using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LockIndicatorHandler : IBinder<GunHandler> {
    public Image cursorImage;
    public RectTransform cursorRect;
    public Camera UICamera;
    public AudioSource audioSource;
    public AudioClip mouseoverSound;
    Color initialColor;
    Collider targetLockCollider;
    float transitionTime;
    float transitionDuration = 0.1f;
    float scaleFactor = 1f;
    float alpha = 1f;
    float effectScale = 0.25f;

    void Start() {
        initialColor = cursorImage.color;
    }
    override public void HandleValueChanged(GunHandler gunHandler) {
        bool uiclick = EventSystem.current.IsPointerOverGameObject();
        if (uiclick) {
            DisableCursorImage();
            return;
        }

        if (gunHandler.HasGun()) {
            CursorData data = gunHandler.currentTargetData;
            if (data == null) {
                DisableCursorImage();
                return;
            }
            if (data.type == CursorData.TargetType.objectLock) {
                if (targetLockCollider != data.targetCollider) {
                    transitionTime = 0f;
                    audioSource.PlayOneShot(mouseoverSound);
                }
                targetLockCollider = data.targetCollider;
                EnableCursorImage(data);
            } else {
                DisableCursorImage();
            }
        } else {
            DisableCursorImage();
        }
    }
    void Update() {
        bool uiclick = EventSystem.current.IsPointerOverGameObject();
        if (uiclick) {
            DisableCursorImage();
            return;
        }

        if (cursorImage.enabled) {
            transitionTime += Time.unscaledDeltaTime;
            if (transitionTime > transitionDuration) {
                scaleFactor = 1f;
            } else {
                scaleFactor = (float)PennerDoubleAnimation.Linear(transitionTime, 1f + effectScale, -1f * effectScale, transitionDuration);
                alpha = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 0f, initialColor.a, transitionDuration);
            }
        }
    }
    void DisableCursorImage() {
        if (targetLockCollider != null) {
        }
        cursorImage.enabled = false;
        targetLockCollider = null;
        // todo: check to reset transition time?
    }
    void EnableCursorImage(CursorData data) {
        Transform root = data.targetCollider.transform.root;
        Rect bounds = Toolbox.GetTotalRenderBoundingBox(root, UICamera);
        cursorImage.enabled = true;
        cursorRect.position = data.screenPosition;
        cursorRect.sizeDelta = new Vector2(bounds.width, bounds.height) * scaleFactor * 1.05f;
        cursorImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
    }
}
