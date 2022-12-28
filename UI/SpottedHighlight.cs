using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public class SpottedHighlight : MonoBehaviour {
    public Transform target;
    public LineRenderer lineRenderer;
    public Material lineMaterial;
    public Transform targetIndicatorTransform;
    public SpriteRenderer targetIndicator;
    public Color red;
    float timer;
    public void Update() {

        // TODO: provide navigation mesh liens

        lineRenderer.enabled = target != null;
        targetIndicator.enabled = target != null;
        if (target == null)
            return;

        timer += Time.unscaledDeltaTime;

        targetIndicatorTransform.position = target.position + 0.05f * Vector3.up;
        Vector3 targetPosition = targetIndicatorTransform.localPosition;
        lineRenderer.SetPosition(1, new Vector3(-1f * targetPosition.z, -1f * targetPosition.x, targetPosition.y));
        // lineRenderer.setpo

        // set targetIndicator color
        targetIndicator.color = red;
        lineRenderer.startColor = red;
        lineRenderer.endColor = red;

        // pulse targetIndicator scale
        targetIndicatorTransform.localScale = Vector3.one * (3 - (0.2f * Mathf.Sin(timer)));

        // pulse material x offset
        float offset = Mathf.Sin(timer);
        lineMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }

    public void SetPoints() {

    }
}
