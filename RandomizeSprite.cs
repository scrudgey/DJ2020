using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeSprite : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public bool flipX;
    public bool flipY;
    void Start() {
        Randomize();
    }

    public void Randomize() {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        if (flipX && Random.Range(0f, 1f) > 0.5f) {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
        if (flipY && Random.Range(0f, 1f) > 0.5f) {
            spriteRenderer.flipY = !spriteRenderer.flipY;
        }
    }

}
