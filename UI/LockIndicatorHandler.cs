using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
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
        Vector3 maxBounds = Vector3.zero;
        float total_min_x = float.MaxValue;
        float total_max_x = float.MinValue;
        float total_min_y = float.MaxValue;
        float total_max_y = float.MinValue;
        Transform root = data.targetCollider.transform.root;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>()) {
            if (renderer is LineRenderer) continue;
            if (renderer.name.ToLower().Contains("jumppoint")) continue;
            if (renderer.name.ToLower().Contains("alerticon")) continue;
            if (renderer.name.ToLower().Contains("shadowcaster")) continue;
            if (renderer.name.ToLower().Contains("blood_spray")) continue;

            Bounds bounds = renderer.bounds;
            if (renderer is SpriteRenderer) {
                SpriteRenderer spriteRenderer = (SpriteRenderer)renderer;
                if (spriteRenderer.sprite != null) {
                    bounds = spriteRenderer.sprite.bounds;

                    // weird hack. maybe because billboarding?
                    Vector3 extents = bounds.extents;
                    extents.y *= 1.3f;
                    bounds.extents = extents;
                }
            }
            // add offset
            bounds.center += renderer.transform.position - root.position;

            Vector3[] screenSpaceCorners = new Vector3[8];
            screenSpaceCorners[0] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[1] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
            screenSpaceCorners[2] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[3] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));

            screenSpaceCorners[4] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[5] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
            screenSpaceCorners[6] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[7] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));

            float min_x = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.x < curMin.x ? x : curMin)).x;
            float max_x = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.x > curMin.x ? x : curMin)).x;
            float min_y = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.y < curMin.y ? x : curMin)).y;
            float max_y = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.y > curMin.y ? x : curMin)).y;

            total_max_x = Mathf.Max(total_max_x, max_x);
            total_min_x = Mathf.Min(total_min_x, min_x);
            total_max_y = Mathf.Max(total_max_y, max_y);
            total_min_y = Mathf.Min(total_min_y, min_y);
        }

        cursorImage.enabled = true;
        cursorRect.position = data.screenPosition;
        cursorRect.sizeDelta = new Vector2(total_max_x - total_min_x, total_max_y - total_min_y) * scaleFactor * 1.05f;

        cursorImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
    }
}
