using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public class RiseAndDissapear : MonoBehaviour, IPoolable {
    private Transform _myTransform;
    public Transform myTransform {
        get {
            if (_myTransform == null) _myTransform = transform;
            return _myTransform;
        }
    }
    public SpriteRenderer spriteRenderer;
    Color initialColor;
    void Awake() {
        initialColor = spriteRenderer.color;
    }
    public void Start() {
        StartCoroutine(DoRiseAndDisappear());
    }
    public void OnPoolActivate() {
        spriteRenderer.color = initialColor;
    }
    public void OnPoolDectivate() {

    }
    IEnumerator DoRiseAndDisappear() {
        float timer = 0f;
        Vector3 initialPosition = myTransform.position;
        float duration = 1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float newY = (float)PennerDoubleAnimation.ExpoEaseOut(timer, initialPosition.y, 0.5f, duration);
            myTransform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
            yield return null;
        }
        timer = 0f;
        duration = 0.5f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float alpha = (float)PennerDoubleAnimation.Linear(timer, 1f, -1f, duration);
            Color color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            spriteRenderer.color = color;
            yield return null;
        }
        PoolManager.I.RecallObject(gameObject);
    }
}
