using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseLight : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    private float timer;
    public float period;
    private Color startColor;
    private float startAlpha;
    private Vector3 startScale;
    void Start() {
        startColor = spriteRenderer.color;
        startAlpha = startColor.a;
        startScale = transform.localScale;
    }
    void Update() {
        timer += Time.deltaTime;
        float intensity = Mathf.Sin(timer * (2 * Mathf.PI / period));
        Color color = new Color(startColor.r, startColor.g, startColor.b, startAlpha * intensity);
        transform.localScale = intensity * startScale;
    }
}
