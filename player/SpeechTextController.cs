using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class SpeechTextController : MonoBehaviour {
    public TextMeshProUGUI textMesh;
    public RectTransform textRect;
    public RectTransform canvasRect;
    public Transform followTransform;
    public Camera cam;
    float visibilityTimer;
    void Start() {
        cam = Camera.main;
        HideText();
        visibilityTimer = 0f;
    }
    void Update() {
        if (visibilityTimer > 0) {
            // SetRectPosition();
            textRect.position = cam.WorldToScreenPoint(followTransform.position);
            visibilityTimer -= Time.deltaTime;
            if (visibilityTimer <= 0) {
                HideText();
            }
        }
    }

    public void Say(string phrase) {
        textMesh.text = phrase;
        visibilityTimer = 5f;
        ShowText();
        // SetRectPosition();
        textRect.position = cam.WorldToScreenPoint(followTransform.position);
    }

    void ShowText() {
        textMesh.enabled = true;
    }
    void HideText() {
        textMesh.enabled = false;
    }
    void SetRectPosition() {
        Rect camRect = cam.pixelRect;

        // world coordinates
        Vector2 orig = followTransform.position;// + offset;

        // viewport coordinates: range from (0, 1)
        Vector2 pos = Camera.main.WorldToViewportPoint(orig);

        // clamp the y coordinate
        pos.y = Mathf.Clamp(pos.y, 0.05f, 0.95f);
        pos.x = Mathf.Clamp(pos.x, 0.05f, 0.95f);

        Vector2 screenPos = new Vector2(
            pos.x * canvasRect.sizeDelta.x,
            pos.y * canvasRect.sizeDelta.y
            );
        // prevent going off screen on the left
        screenPos.x = Mathf.Max(screenPos.x, textRect.rect.width / 2f);

        // prevent going off screen on the right
        screenPos.x = Mathf.Min(screenPos.x, canvasRect.rect.width - (textRect.rect.width / 2f));

        // prevent ging below the screen
        screenPos.y = Mathf.Max(screenPos.y, textRect.rect.height / 2f);

        // prevent going above the screen
        screenPos.y = Mathf.Min(screenPos.y, camRect.height - (textRect.rect.height / 2f));

        textRect.position = screenPos;
        // textRect.anchoredPosition = screenPos;
    }
}
