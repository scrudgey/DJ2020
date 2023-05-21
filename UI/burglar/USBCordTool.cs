using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
public class USBCordTool : MonoBehaviour {
    public UILineRenderer uILineRenderer;
    public RectTransform toolTip;
    public RectTransform anchor;
    Catenary catenary;
    Coroutine slackRoutine;

    float slack;
    float slackFactor;

    void Start() {
        slack = 100f;
        slackFactor = 1f;
        catenary = new Catenary {
            start = toolTip.position,
            end = anchor.position,
            steps = 10,
            slack = 100
        };
    }
    void Update() {
        uILineRenderer.Points = GetPoints();
    }

    public void Slacken(bool value) {
        if (slackRoutine != null) {
            StopCoroutine(slackRoutine);
        }
        slackRoutine = StartCoroutine(DoSlack(value));
    }
    IEnumerator DoSlack(bool value) {
        float timer = 0f;
        float duration = 0.5f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            if (value) {
                slack = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 100f, 100f, duration);
            } else {
                slack = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 200f, -100f, duration);
            }
            yield return null;
        }
        slackRoutine = null;
    }

    Vector2[] GetPoints() {
        Vector3 start = toolTip.position;
        start.y -= 40f;

        slackFactor = 1000f / (toolTip.position - anchor.position).magnitude;
        Debug.Log($"{slackFactor}");

        catenary.slack = slack * slackFactor;
        catenary.start = start;
        catenary.end = (anchor.position / 2f) + (toolTip.position / 2f);

        List<Vector2> points = catenary.Points()
            .Select(point => new Vector2(point.x - toolTip.position.x, point.y - toolTip.position.y))
            .ToList();

        points.Insert(0, Vector2.zero);
        return points.ToArray();
    }
}
