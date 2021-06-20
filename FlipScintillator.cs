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
    public void Start() {
        Flip();
    }
    public void Flip() {
        if (flipX) {
            if (Random.value >= 0.5) {
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }
        }
        if (flipY) {
            if (Random.value >= 0.5) {
                spriteRenderer.flipY = !spriteRenderer.flipY;
            }
        }
    }
    public void Update() {
        if (dutyCycle <= 0)
            return;
        if (scaleWithVelocity) {
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
