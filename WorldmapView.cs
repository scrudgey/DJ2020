using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class WorldmapView : MonoBehaviour {
    public Image mapImage;
    public Color mapColor1;
    public Color mapColor2;
    public float timer;
    public float colorFlipInterval;

    public TextMeshProUGUI mapDescriptionTitle;
    public TextMeshProUGUI mapDescriptionBody;
    public TextMeshProUGUI mapDescriptionTarget;

    [Header("points")]
    public RectTransform[] points;
    public RectTransform circle;
    public Image circleImage;

    Coroutine circleRoutine;

    void Start() {
        StopHighlight();
    }
    public void Initialize() {
        mapDescriptionTitle.text = "";
        mapDescriptionBody.text = "";
    }
    public void ShowText() {
        StartCoroutine(Toolbox.BlitText(mapDescriptionTitle, "NEO BOSTON"));
        StartCoroutine(Toolbox.BlitText(mapDescriptionBody, "Pop: 2,654,776\n42°21′37″N\n71°3′28″W"));
    }
    void Update() {
        timer += Time.unscaledDeltaTime;
        if (timer > colorFlipInterval) {
            timer -= colorFlipInterval;
            if (mapImage.color == mapColor1) {
                mapImage.color = mapColor2;
            } else {
                mapImage.color = mapColor1;
            }
        }
    }

    public void HighlightPoint(int index) {
        StopCircle();
        if (index > 0 && index < points.Length) {
            circleImage.enabled = true;
            Color color = circleImage.color;
            circle.anchoredPosition = points[index].anchoredPosition;
            circleRoutine = StartCoroutine(Toolbox.Ease(null, 1f, 0f, 1f, PennerDoubleAnimation.Linear, (amount) => {
                circle.localScale = amount * Vector3.one;
                circleImage.color = new Color(color.r, color.g, color.b, 1f - amount);
            }, unscaledTime: true, looping: true));
        }
    }
    public void StopHighlight() {
        StopCircle();
        circleImage.enabled = false;
    }
    void StopCircle() {
        if (circleRoutine != null) {
            StopCoroutine(circleRoutine);
        }
    }
}
