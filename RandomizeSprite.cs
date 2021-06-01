using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeSprite : MonoBehaviour {
    public Sprite[] sprites;
    public bool flipX;
    public bool flipY;
    public float scalemagnitude;
    void Start() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];

        Vector3 scale = Vector3.one;
        if (flipX && Random.Range(0f, 1f) > 0.5f) {
            scale.x = -1f;
        }
        if (flipY && Random.Range(0f, 1f) > 0.5f) {
            scale.y = -1f;
        }
        transform.localScale = scale * scalemagnitude;
    }

}
