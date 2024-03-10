using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipScintillator : MonoBehaviour {
    public bool flipX = true;
    public bool flipY = true;
    public float dutyCycle = 0;
    public float timer;
    public bool scaleWithVelocity;
    public Rigidbody body;
    public SpriteRenderer spriteRenderer;
    public Transform flipTransform;
    public void Start() {
        Flip();
    }
    public void Flip() {
        if (flipX) {
            if (spriteRenderer != null)
                if (Random.value >= 0.5) {
                    spriteRenderer.flipX = !spriteRenderer.flipX;
                }
            if (flipTransform != null) {
                Vector3 scale = flipTransform.localScale;
                scale.x *= -1f;
                flipTransform.localScale = scale;
            }
        }
        if (flipY) {
            if (spriteRenderer != null)
                if (Random.value >= 0.5) {
                    spriteRenderer.flipY = !spriteRenderer.flipY;
                }
            if (flipTransform != null) {
                Vector3 scale = flipTransform.localScale;
                scale.y *= -1f;
                flipTransform.localScale = scale;
            }
        }
    }
    public void Update() {
        if (dutyCycle <= 0)
            return;
        if (scaleWithVelocity && body != null) {
            timer += Time.deltaTime * (body.velocity.magnitude);
        } else {
            timer += Time.deltaTime;
        }
        if (timer > dutyCycle) {
            timer = 0;
            Flip();
        }
    }
}
