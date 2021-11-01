using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

// TODO: pool line objects
public class BulletFX : MonoBehaviour {
    public enum FadeStyle { none, timer, count, invisible, streak }
    public FadeStyle fadeStyle;
    public LineRenderer lineRenderer;
    public int countLimit = 50;
    public float fadetime = 0.1f;
    float timer;
    Color baseColor = Color.white;
    float startStreak = 0.4f;
    float endStreak = 0.7f;
    float streakWidth = 0;
    void Start() {
        baseColor = lineRenderer.startColor;
    }
    public void Initialize(FadeStyle newStyle, Vector3 startPoint, Vector3 endPoint) {
        this.fadeStyle = newStyle;
        if (fadeStyle == FadeStyle.count) {
            lineRenderer.SetPositions(new Vector3[] { startPoint, endPoint });
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
        } else if (fadeStyle == FadeStyle.invisible) {
            lineRenderer.startColor = Color.clear;
            lineRenderer.endColor = Color.clear;
        } else if (fadeStyle == FadeStyle.streak) {

            Vector3 direction = (endPoint - startPoint).normalized;

            startStreak = Random.Range(0.25f, 0.35f);
            streakWidth = Random.Range(0.2f, 0.35f);
            endStreak = Mathf.Min(startStreak + streakWidth, 1f);

            SetStreakGradient(lineRenderer.startColor);
            lineRenderer.positionCount = 11;
            lineRenderer.SetPosition(0, startPoint);

            Vector3 delta = direction * (endPoint - startPoint).magnitude / 10f;
            for (int i = 1; i < 10; i++) {
                lineRenderer.SetPosition(i, startPoint + (i * delta));
            }
            lineRenderer.SetPosition(10, endPoint);
        }
    }
    void Update() {
        if (fadeStyle == FadeStyle.timer || fadeStyle == FadeStyle.streak) {

            timer += Time.deltaTime;
            startStreak = endStreak;
            endStreak = Mathf.Min(startStreak + streakWidth, 1f);

            float alpha = (float)PennerDoubleAnimation.BackEaseIn(timer, 1f, -1f, fadetime);
            Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            if (fadeStyle == FadeStyle.timer) {
                lineRenderer.startColor = newColor;
                lineRenderer.endColor = newColor;
            } else if (fadeStyle == FadeStyle.streak) {
                SetStreakGradient(newColor);
            }

            if (timer > fadetime) {
                Destroy(gameObject);
            }
        }
    }

    public void SetStreakGradient(Color newColor) {
        Gradient gradient = new Gradient();

        // TODO: get length of ray from line renderer points

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = newColor;
        colorKey[0].time = 0.0f;
        colorKey[1].color = newColor;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[5];
        alphaKey[0].alpha = 0.0f;
        alphaKey[0].time = 0.0f;

        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = startStreak;

        alphaKey[2].alpha = 1.0f;
        alphaKey[2].time = (startStreak + endStreak) / 2f;

        alphaKey[3].alpha = 0.0f;
        alphaKey[3].time = endStreak;

        alphaKey[4].alpha = 0.0f;
        alphaKey[4].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
        lineRenderer.colorGradient = gradient;
    }
}
