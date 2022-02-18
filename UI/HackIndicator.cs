using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HackIndicator : MonoBehaviour, IBinder<HackController> {
    public HackController target { get; set; }
    public Image image;
    public RectTransform imageRect;
    public Camera cam;
    public Color color;
    public float timer;

    void Start() {
        image.color = color;
        ((IBinder<HackController>)this).Bind(HackController.I.gameObject);
    }

    public void HandleValueChanged(HackController hackController) {
        if (hackController.targets.Count == 0) {
            image.enabled = false;
        } else {
            image.enabled = true;
            HackController.HackData data = hackController.targets[0];
            Vector3 screenPoint = cam.WorldToScreenPoint(data.node.position);
            imageRect.position = screenPoint;
        }
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer < 0.25) {
            imageRect.sizeDelta = new Vector2(50f, 50f);
        } else if (timer < 0.5) {
            imageRect.sizeDelta = new Vector2(35f, 35f);
        } else if (timer < 0.75) {
            imageRect.sizeDelta = new Vector2(25f, 25f);
        } else {
            timer = 0f;
        }
    }
}
