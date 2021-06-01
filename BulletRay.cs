using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class BulletRay : MonoBehaviour {
    public enum FadeStyle { none, timer, count, invisible, streak }
    public FadeStyle fadeStyle;
    public LineRenderer lineRenderer;
    public int countLimit = 50;
    public float fadetime = 1f;
    float timer;
    Color baseColor = Color.white;
    void Start() {
        baseColor = lineRenderer.startColor;
    }
    public void SetFadeStyle(FadeStyle newStyle) {
        this.fadeStyle = newStyle;
        if (fadeStyle == FadeStyle.count) {
            BulletRay[] rays = FindObjectsOfType<BulletRay>();
            while (rays.Length > countLimit) {
                BulletRay ray = rays[Random.Range(0, rays.Length)];
                if (ray != this) {
                    DestroyImmediate(ray.gameObject);
                }
                rays = FindObjectsOfType<BulletRay>();
            }
        } else if (fadeStyle == FadeStyle.invisible) {
            lineRenderer.startColor = Color.clear;
            lineRenderer.endColor = Color.clear;
        }
    }
    void Update() {
        if (fadeStyle == FadeStyle.timer || fadeStyle == FadeStyle.streak) {
            timer += Time.deltaTime;
            float alpha = (float)PennerDoubleAnimation.BackEaseIn(timer, 1f, -1f, fadetime);
            Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            if (fadeStyle == FadeStyle.timer) {
                lineRenderer.startColor = newColor;
                lineRenderer.endColor = newColor;
            } else if (fadeStyle == FadeStyle.streak) {
                Gradient gradient = new Gradient();

                // Populate the color keys at the relative time 0 and 1 (0 and 100%)
                GradientColorKey[] colorKey = new GradientColorKey[2];
                colorKey[0].color = newColor;
                colorKey[0].time = 0.0f;
                colorKey[1].color = newColor;
                colorKey[1].time = 1.0f;

                // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
                alphaKey[0].alpha = 0.0f;
                alphaKey[0].time = 0.0f;

                alphaKey[1].alpha = 1.0f;
                alphaKey[1].time = 0.5f;

                alphaKey[2].alpha = 0.0f;
                alphaKey[2].time = 1.0f;

                gradient.SetKeys(colorKey, alphaKey);
                lineRenderer.colorGradient = gradient;

            }

            if (timer > fadetime) {
                Destroy(gameObject);
            }
        }
    }
}
