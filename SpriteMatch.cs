using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMatch : MonoBehaviour {
    public SpriteRenderer from;
    public SpriteRenderer to;

    void Start() {
        to.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }
    public void Update() {
        if (to == null || from == null) return;
        to.sprite = from.sprite;
        to.flipX = from.flipX;
    }
}
