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
    float slack;
    float slackFactor;
    bool extraSlack;

    public float originalSlack = 100f;
    public float slackDelta = 50f;

    void Start() {
        slack = originalSlack;
        slackFactor = 1f;
        catenary = new Catenary {
            start = toolTip.position,
            end = anchor.position,
            steps = 10,
            slack = originalSlack
        };
    }
    void Update() {
        uILineRenderer.Points = GetPoints();
    }

    public void Slacken(bool value) {
        extraSlack = value;
    }

    Vector2[] GetPoints() {
        Vector3 start = toolTip.position;
        start.y -= 40f;

        slackFactor = 1000f / (toolTip.position - anchor.position).magnitude;
        slackFactor = Mathf.Clamp(slackFactor, 0.3f, 5f);
        float targetSlack = extraSlack ? (originalSlack + slackDelta) * slackFactor : originalSlack * slackFactor;

        slack = Mathf.Lerp(slack, targetSlack, 0.05f);

        catenary.slack = slack;
        catenary.start = start;
        catenary.end = anchor.position;

        List<Vector2> points = catenary.Points()
            .Select(point => new Vector2(point.x - toolTip.position.x, point.y - toolTip.position.y))
            .ToList();

        points.Insert(0, Vector2.zero);
        return points.ToArray();
    }
}
