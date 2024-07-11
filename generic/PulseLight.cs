using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PulseLight : MonoBehaviour {
    public enum PulseType { fadeInOut, triangle }
    public PulseType type;
    public SpriteRenderer[] spriteRenderers;
    private float timer;
    public float period;
    private Color[] startColors;
    private float[] startAlphas;
    private Vector3[] startScales;
    public bool randomizeInitialScale = true;
    private bool _doPulse = true;
    public bool doPulse {
        get { return _doPulse; }
        set {
            _doPulse = value;
            foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
                spriteRenderer.enabled = _doPulse;
            }
        }
    }
    void Start() {
        if (randomizeInitialScale) {
            transform.localScale = Random.Range(0.9f, 1.3f) * Vector3.one;
            timer = Random.Range(0, period);
        }
        startColors = spriteRenderers.Select(s => s.color).ToArray();
        startAlphas = spriteRenderers.Select(s => s.color.a).ToArray();
        startScales = spriteRenderers.Select(s => s.transform.localScale).ToArray();
    }
    void Update() {
        if (doPulse) {
            timer += Time.unscaledDeltaTime;
            float intensity = 0f;
            if (type == PulseType.fadeInOut) {
                intensity = Mathf.Sin(timer * (2 * Mathf.PI / period));
            } else if (type == PulseType.triangle) {
                if (timer > period) {
                    timer -= period;
                }
                intensity = (timer * (-1f / period)) + 1f;
            }
            SetIntensity(intensity);
        }
    }

    void SetIntensity(float intensity) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            Color color = new Color(startColors[i].r, startColors[i].g, startColors[i].b, startAlphas[i] * intensity);
            spriteRenderers[i].transform.localScale = intensity * startScales[i];
        }
    }
}
