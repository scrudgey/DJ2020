using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Decor : MonoBehaviour {
    // public Sprite[] spritesheet;
    public int index;
    void Start() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Sprite[] spritesheet = Resources.LoadAll<Sprite>("sprites/decor/decor") as Sprite[];
        spriteRenderer.sprite = spritesheet[index];
    }
}
