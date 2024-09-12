using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class IndicatorUIController : MonoBehaviour {
    public enum Origin { top, bottom }
    public enum Direction { right, up, left, down }
    public Canvas myCanvas;
    public RectTransform[] indicatorRectTransforms;
    Dictionary<RectTransform, Image> indicators;
    public RectTransform lineRendererTopOrigin;
    public RectTransform lineRendererBottomOrigin;
    // public Image[] indicators;
    [Header("specifics")]
    public RectTransform dialogueIndicator;
    public RectTransform cyberInfoIndicatorAnchor;
    public RectTransform hackIndicatorAnchor;
    public RectTransform hackButtonIndicator;
    public RectTransform deployButtonIndicator;
    public RectTransform downloadButtonIndicator;
    public RectTransform handButtonIndicator;
    public UILineRenderer lineRenderer;
    public RectTransform lineRendererRectTransform;
    public RectTransform keyMenuIndicator;
    public RectTransform lockedCybernodeIndicator;
    [Header("burglar positions")]
    public RectTransform probeIndicator;
    public RectTransform lockpickIndicator;
    public RectTransform screwdriverIndicator;
    public RectTransform wirecuttersIndicator;

    Coroutine coroutine;
    Coroutine lineCoroutine;
    // Vector3 offset;
    RectTransform[] targets;
    // RectTransform targetRectTransform;
    Vector3[] offsets;
    UIController uiController;
    Dictionary<RectTransform, Image> targetToIndicator;
    Direction[] directions;
    void Start() {
        indicators = indicatorRectTransforms.ToDictionary(rect => rect, rect => rect.GetComponentInChildren<Image>());
    }
    public void DrawLine(RectTransform target, Vector3 offset, UIController uIController, Origin origin = Origin.top) {
        if (lineCoroutine != null) {
            StopCoroutine(lineCoroutine);
        }
        this.uiController = uIController;
        lineCoroutine = StartCoroutine(drawLine(target.position + offset, origin));
    }
    public void ShowIndicators(RectTransform[] targets, Vector3[] offsets, Direction[] directions) {
        HideAllIndicators();
        this.directions = directions;
        this.offsets = offsets;
        this.targets = targets;

        if (targets.Length > 0) {
            Canvas otherCanvas = targets[0].GetComponentInParent<Canvas>();
            myCanvas.sortingOrder = otherCanvas.sortingOrder + 1;
        }

        for (int i = 0; i < targets.Length; i++) {
            RectTransform target = targets[i];
            RectTransform indicatorRectTransform = indicatorRectTransforms[i];
            Image indicator = indicators[indicatorRectTransform];

            Direction direction = directions[i];

            indicator.enabled = true;
            float zRotation = Direction2Rotation(direction);
            indicatorRectTransform.rotation = Quaternion.Euler(0f, 0f, zRotation);

            targetToIndicator[target] = indicator;
        }

        coroutine = StartCoroutine(doIndicate());
    }
    float Direction2Rotation(Direction direction) => direction switch {
        Direction.right => -90f,
        Direction.up => 0f,
        Direction.left => 90f,
        Direction.down => 180f
    };

    public void HideAllIndicators() {
        targetToIndicator = new Dictionary<RectTransform, Image>();
        lineRenderer.enabled = false;
        foreach (Image indicator in indicators.Values) {
            indicator.enabled = false;
        }
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        if (lineCoroutine != null) {
            StopCoroutine(lineCoroutine);
        }
    }
    public void HideIndicator(RectTransform target) {
        Image indicator = targetToIndicator[target];
        indicator.enabled = false;
    }

    IEnumerator doIndicate() {
        yield return Toolbox.Ease(null, 1f, 10f, 0f, PennerDoubleAnimation.BounceEaseOut, (amount) => {

            for (int i = 0; i < targets.Length; i++) {
                RectTransform target = targets[i];
                if (target == null) continue;
                RectTransform indicatorRectTransform = indicatorRectTransforms[i];
                Image indicator = indicators[indicatorRectTransform];
                Direction direction = directions[i];

                Vector3 basePosition = target.position + offsets[i];
                Vector3 directionalOffset = amount * (direction switch {
                    Direction.right => Vector3.left,
                    Direction.up => Vector3.down,
                    Direction.left => Vector3.right,
                    Direction.down => Vector3.up
                });
                indicatorRectTransform.position = basePosition + directionalOffset;
            }
        }, unscaledTime: true, looping: true);
    }

    IEnumerator drawLine(Vector3 basePosition, Origin origin) {
        Vector3 originPosition = origin switch {
            Origin.bottom => lineRendererBottomOrigin.position,
            Origin.top => lineRendererTopOrigin.position
        };
        lineRendererRectTransform.position = originPosition;

        Vector2 lineRendererTarget = basePosition - lineRendererRectTransform.position;
        float distance = lineRendererTarget.magnitude;
        Vector3 direction = lineRendererTarget.normalized;

        yield return Toolbox.Ease(null, 0.5f, 0f, distance, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            Vector2 point = (direction * amount);
            Vector2[] points = new Vector2[2] { Vector2.zero, point };
            lineRenderer.Points = points;
            lineRenderer.enabled = true;
        }, unscaledTime: true);

        while (uiController.cutsceneDialogueEnabled) {
            lineRenderer.Points = new Vector2[2] { Vector2.zero, lineRendererTarget };
            yield return null;
        }
        lineRenderer.enabled = false;
    }

}
