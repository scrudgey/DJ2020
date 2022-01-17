using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PowerOverlay : MonoBehaviour {
    public Camera cam;
    public GameObject powerNodeIndicatorPrefab;
    public LineRenderer lineRenderer;

    Dictionary<PowerNode, PowerNodeIndicator> indicators = new Dictionary<PowerNode, PowerNodeIndicator>();

    public void DisableOverlay() {
        lineRenderer.enabled = false;
        indicators = new Dictionary<PowerNode, PowerNodeIndicator>();
    }

    void Update() {
        foreach (KeyValuePair<PowerNode, PowerNodeIndicator> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            // kvp.Value.position = screenPoint;
            kvp.Value.Configure(kvp.Key);
            kvp.Value.SetScreenPosition(screenPoint);
        }
    }
    public void Refresh(PowerGraph graph) {

        if (graph == null) {
            DisableOverlay();
            return;
        }

        List<Vector3> positions = new List<Vector3>();
        foreach (PowerNode node in graph.nodes.Values) {

            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            PowerNodeIndicator indicator = GetIndicator(node);
            indicator.Configure(node);
            indicator.SetScreenPosition(screenPoint);
            // indicator.position = screenPoint;

            foreach (PowerNode neighbor in graph.Neighbors(node)) {
                positions.Add(node.position);
                positions.Add(neighbor.position);
            }
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    PowerNodeIndicator GetIndicator(PowerNode node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            GameObject newIndicator = GameObject.Instantiate(powerNodeIndicatorPrefab);
            PowerNodeIndicator indicator = newIndicator.GetComponent<PowerNodeIndicator>();
            indicator.Configure(node);
            indicator.rectTransform.SetParent(transform, false);
            indicators[node] = indicator;
            return indicator;
        }
    }
}
