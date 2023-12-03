using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.AI;
public class SpottedHighlight : MonoBehaviour {
    public Transform target;
    public NavMeshPath navMeshPath;
    public LineRenderer lineRenderer;
    public Material lineMaterial;
    public Transform followTransform;
    public Transform targetIndicatorTransform;
    public SpriteRenderer targetIndicator;
    public Color red;
    float timer;
    public void FixedUpdate() {
        lineRenderer.enabled = target != null;
        targetIndicator.enabled = target != null;
        if (target == null)
            return;

        timer += Time.unscaledDeltaTime;

        targetIndicatorTransform.position = target.position + 0.05f * Vector3.up;
        SetPoints();

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
        if (navMeshPath == null) {
            lineRenderer.useWorldSpace = false;
            Vector3 targetPosition = targetIndicatorTransform.localPosition;
            lineRenderer.SetPosition(1, new Vector3(-1f * targetPosition.z, -1f * targetPosition.x, targetPosition.y));
        } else {
            lineRenderer.useWorldSpace = true;
            List<Vector3> points = navMeshPath.corners.Select(point => point + new Vector3(0f, 0.1f, 0f)).ToList();
            points[0] = followTransform.position;
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }
}
