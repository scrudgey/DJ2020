using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationHighlight : MonoBehaviour {
    public Vector3 target;
    public Transform targetIndicatorTransform;
    public SpriteRenderer targetIndicator;
    public Color red;
    float timer;
    public void Update() {
        targetIndicator.enabled = target != null;
        if (target == null)
            return;

        timer += Time.unscaledDeltaTime;

        targetIndicatorTransform.position = target + 0.05f * Vector3.up;

        // set targetIndicator color
        targetIndicator.color = red;

        // pulse targetIndicator scale
        targetIndicatorTransform.localScale = Vector3.one * (3 - (0.2f * Mathf.Sin(timer)));

        // pulse material x offset
        float offset = Mathf.Sin(timer);
    }
}
