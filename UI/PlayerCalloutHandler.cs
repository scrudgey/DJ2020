using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class PlayerCalloutHandler : MonoBehaviour {
    public Image cursorImage;
    public RectTransform cursorRect;
    // public RectTransform cursorMaskRect;
    public Camera UICamera;
    public AudioSource audioSource;
    public AudioClip activateSound;
    // Color initialColor;
    public Color highlightColor;
    public Color baseColor;
    float transitionTime;
    float blinkTimer;
    bool doBlink;
    float blinkInterval = 0.05f;
    float easeSizeDuration = 0.75f;
    float blinkDuration = 0.5f;
    float hangDuration = 3f;
    float scaleFactor = 1f;
    float alpha = 1f;
    float effectScale = 2f;
    GameObject target;
    bool active;
    void Start() {
        GameManager.OnFocusChanged += ActivatePlayerCallout;
        GameManager.OnInputModeChange += HandleInputModeChange;
    }
    void OnDestroy() {
        GameManager.OnFocusChanged -= ActivatePlayerCallout;
        GameManager.OnInputModeChange -= HandleInputModeChange;
    }
    public void HandleInputModeChange(InputMode fromMode, InputMode toMode) {
        if (fromMode == InputMode.aim && toMode == InputMode.gun) {
            ActivatePlayerCallout(GameManager.I.playerObject);
        }
        if (fromMode == InputMode.aim && toMode == InputMode.gun) {
            ActivatePlayerCallout(GameManager.I.playerObject);
        }
    }

    // TODO: long and short player callouts
    public void ActivatePlayerCallout(GameObject playerObject) {
        active = true;
        target = playerObject;
        cursorImage.enabled = true;
        transitionTime = 0f;
        scaleFactor = 1f + effectScale;
        alpha = 0f;
        audioSource.PlayOneShot(activateSound);
        SetScale();
    }
    void DisablePlayerCallout() {
        cursorImage.enabled = false;
        active = false;
    }
    void Update() {
        if (active) {
            transitionTime += Time.deltaTime; // TODO: change to unscaled
            if (transitionTime < easeSizeDuration) {
                cursorImage.enabled = true;

                scaleFactor = (float)PennerDoubleAnimation.CircEaseOut(transitionTime, 1f + effectScale, -1f * effectScale, easeSizeDuration);
                alpha = (float)PennerDoubleAnimation.CircEaseOut(transitionTime, 0f, baseColor.a, easeSizeDuration);
                Debug.Log(alpha);
                SetScale();
            } else if (transitionTime > easeSizeDuration && transitionTime < easeSizeDuration + blinkDuration) {
                scaleFactor = 1f;
                alpha = baseColor.a;
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval) {
                    blinkTimer -= blinkInterval;
                    doBlink = !doBlink;
                }
                SetScale();
                if (doBlink) {
                    // cursorImage.enabled = false;
                    cursorImage.color = highlightColor;
                } else {
                    cursorImage.color = baseColor;
                    // cursorImage.enabled = true;
                }
            } else if (transitionTime > easeSizeDuration + blinkDuration && transitionTime < easeSizeDuration + blinkDuration + hangDuration) {
                cursorImage.enabled = true;
                SetScale();
            } else {
                Debug.Log("end sequence");
                cursorImage.enabled = false;
                active = false;
            }
        }
    }
    public void SetScale() {
        Transform root = target.transform;
        Rect bounds = Toolbox.GetTotalRenderBoundingBox(root, UICamera);
        cursorRect.position = UICamera.WorldToScreenPoint(root.position) + new Vector3(0f, bounds.height / 2f, 0f);
        cursorRect.sizeDelta = new Vector2(bounds.width, bounds.height) * scaleFactor * 1.05f;
        cursorImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}


// zoom in
// arrive at bounds
// blink / flash
// hang
// disable