using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class HackIndicator : MonoBehaviour, IBinder<HackController> {
    public HackController target { get; set; }
    public Image image;
    public RectTransform imageRect;
    public Camera cam;
    public Color color;
    public float timer;
    public VulnerabilityIndicator vulnerabilityIndicator;
    bool blinkImage;
    bool showImage;
    CyberNode nodeTarget;

    void Start() {
        image.color = color;
        ((IBinder<HackController>)this).Bind(HackController.I.gameObject);
        HandleValueChanged(HackController.I);
    }

    public void HandleValueChanged(HackController hackController) {
        // TODO: multiple hacks and vulnerable nodes simultaneously  
        if (hackController == null) {
            return;
        } else if (hackController?.targets?.Count > 0) {
            showImage = true;
            HackController.HackData data = hackController.targets[0];
            nodeTarget = data.node;

            vulnerabilityIndicator.Disable();
        } else if (hackController?.vulnerableManualNodes?.Count != 0) {
            showImage = false;
            nodeTarget = hackController.vulnerableManualNodes[0];
            // nodeTarget = hackController.vulnerableManualNodes[0];
            // Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            // imageRect.position = screenPoint;
            vulnerabilityIndicator.Enable();
        } else if (hackController.vulnerableNetworkNode != null) {
            showImage = false;
            nodeTarget = hackController.vulnerableNetworkNode;
            // Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            // imageRect.position = screenPoint;
            vulnerabilityIndicator.Enable();
        } else {
            showImage = false;
            vulnerabilityIndicator.Disable();
        }
    }

    void Update() {
        timer += Time.deltaTime;
        if (showImage) {
            image.enabled = !blinkImage;
        } else image.enabled = false;
        if (timer < 0.25) {
            Vector2 size = new Vector2(50f, 50f);
            SetSize(size);
        } else if (timer < 0.5) {
            Vector2 size = new Vector2(35f, 35f);
            SetSize(size);
        } else if (timer < 0.75) {
            Vector2 size = new Vector2(25f, 25f);
            SetSize(size);
        } else {
            timer = 0f;
        }
        if (nodeTarget != null)
            SetPosition();
    }
    void SetPosition() {
        Vector3 screenPoint = cam.WorldToScreenPoint(nodeTarget.position);
        imageRect.position = screenPoint;
    }
    void SetSize(Vector2 size) {
        if (imageRect.rect.size != size) {
            StartCoroutine(BlinkImage());
        }
        imageRect.sizeDelta = size;
    }

    public IEnumerator BlinkImage(float blinkInterval = 0.05f) {
        blinkImage = true;
        float blinkTimer = 0;
        while (blinkTimer < blinkInterval) {
            blinkTimer += Time.unscaledDeltaTime;
            yield return null;
        }
        blinkImage = false;
    }
}
