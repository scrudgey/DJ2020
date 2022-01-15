using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PowerOverlay : MonoBehaviour {
    public Camera cam;
    public GameObject powerNodeIndicatorPrefab;
    public LineRenderer lineRenderer;

    Dictionary<PowerNode, RectTransform> indicators = new Dictionary<PowerNode, RectTransform>();

    public void DisableOverlay() {
        lineRenderer.enabled = false;
    }

    void Update() {
        foreach (KeyValuePair<PowerNode, RectTransform> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            Debug.Log(screenPoint);
            kvp.Value.position = screenPoint;
            Debug.Log($"{kvp.Key} {screenPoint} {kvp.Value.anchoredPosition}");
        }
    }
    public void Refresh(PowerGraph graph) {

        if (graph == null) {
            DisableOverlay();
            return;
        }

        List<Vector3> positions = new List<Vector3>();
        foreach (PowerNode node in graph.nodes.Values) {

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(node.position);
            RectTransform indicator = GetIndicator(node);
            indicator.position = screenPoint;

            foreach (PowerNode neighbor in graph.Neighbors(node)) {
                positions.Add(node.position);
                positions.Add(neighbor.position);
            }
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    RectTransform GetIndicator(PowerNode node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            GameObject newIndicator = GameObject.Instantiate(powerNodeIndicatorPrefab);
            RectTransform rt = newIndicator.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            indicators[node] = rt;
            return rt;
        }
    }
}
