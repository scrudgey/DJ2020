using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
public class SpeechTextController : MonoBehaviour {
    public bool scaleWithPlayerDistance;
    public TextMeshProUGUI textMesh;
    // public RectTransform textRect;
    public List<RectTransform> childRects;
    public RectTransform canvasRect;
    public Transform followTransform;
    public Camera cam;
    float visibilityTimer;
    float haltSpeechTimeout;
    readonly static float FALLOFF_DISTANCE = 10f;
    void Start() {
        cam = Camera.main;
        HideText();
        visibilityTimer = 0f;
        SetRectPositions();
    }
    void Update() {
        if (haltSpeechTimeout > 0) {
            haltSpeechTimeout -= Time.deltaTime;
            HideText();
            return;
        }
        if (visibilityTimer > 0) {
            SetRectPositions();
            visibilityTimer -= Time.deltaTime;
            if (visibilityTimer <= 0) {
                HideText();
            }
        }
        if (scaleWithPlayerDistance && IsSpeaking()) {
            Vector3 displacement = followTransform.position - GameManager.I.playerPosition;
            float distance = displacement.magnitude;
            if (distance > FALLOFF_DISTANCE) {
                textMesh.transform.localScale = Vector3.zero;
            } else {
                textMesh.transform.localScale = new Vector3(1f, 1.2f, 1f) * (float)PennerDoubleAnimation.ExpoEaseOut(distance, 2f, -2f, FALLOFF_DISTANCE);
            }
        }
    }
    public void HaltSpeechForTime(float timeout) {
        HideText();
    }

    public void Say(string phrase) {
        textMesh.text = phrase;
        visibilityTimer = 5f;
        ShowText();
    }
    void SetRectPositions() {
        childRects.ForEach((rect) => rect.position = cam.WorldToScreenPoint(followTransform.position));
    }

    void ShowText() {
        textMesh.enabled = true;
        SetRectPositions();
    }
    public void HideText() {
        textMesh.enabled = false;
    }
    public bool IsSpeaking() {
        return textMesh.enabled;
    }
    // void SetRectPosition() {
    //     Rect camRect = cam.pixelRect;

    //     // world coordinates
    //     Vector2 orig = followTransform.position;// + offset;

    //     // viewport coordinates: range from (0, 1)
    //     Vector2 pos = Camera.main.WorldToViewportPoint(orig);

    //     // clamp the y coordinate
    //     pos.y = Mathf.Clamp(pos.y, 0.05f, 0.95f);
    //     pos.x = Mathf.Clamp(pos.x, 0.05f, 0.95f);

    //     Vector2 screenPos = new Vector2(
    //         pos.x * canvasRect.sizeDelta.x,
    //         pos.y * canvasRect.sizeDelta.y
    //         );
    //     // prevent going off screen on the left
    //     screenPos.x = Mathf.Max(screenPos.x, textRect.rect.width / 2f);

    //     // prevent going off screen on the right
    //     screenPos.x = Mathf.Min(screenPos.x, canvasRect.rect.width - (textRect.rect.width / 2f));

    //     // prevent ging below the screen
    //     screenPos.y = Mathf.Max(screenPos.y, textRect.rect.height / 2f);

    //     // prevent going above the screen
    //     screenPos.y = Mathf.Min(screenPos.y, camRect.height - (textRect.rect.height / 2f));

    //     textRect.position = screenPos;
    //     // textRect.anchoredPosition = screenPos;
    // }
}
