using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PowerOverlay : MonoBehaviour {
    public UIColorSet colorSet;
    public Camera cam;
    public GameObject powerNodeIndicatorPrefab;
    PowerGraph graph;

    Dictionary<PowerNode, PowerNodeIndicator> indicators = new Dictionary<PowerNode, PowerNodeIndicator>();
    void Awake() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
    public void DisableOverlay() {
        indicators = new Dictionary<PowerNode, PowerNodeIndicator>();
    }

    void Update() {
        foreach (KeyValuePair<PowerNode, PowerNodeIndicator> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            kvp.Value.Configure(kvp.Key, graph);
            kvp.Value.SetScreenPosition(screenPoint);
        }
    }
    public void Refresh(PowerGraph graph) {
        this.graph = graph;
        // Debug.Log($"refreshing graph {graph}");
        if (graph == null || cam == null) {
            DisableOverlay();
            return;
        }

        foreach (PowerNode node in graph.nodes.Values) {
            PowerNodeIndicator indicator = GetIndicator(node);

            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            indicator.SetScreenPosition(screenPoint);
            indicator.Configure(node, graph);
        }
    }

    PowerNodeIndicator GetIndicator(PowerNode node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            GameObject newIndicator = GameObject.Instantiate(powerNodeIndicatorPrefab);
            PowerNodeIndicator indicator = newIndicator.GetComponent<PowerNodeIndicator>();
            indicator.SetScreenPosition(screenPoint);
            indicator.Configure(node, graph);
            indicator.rectTransform.SetParent(transform, false);
            indicators[node] = indicator;
            return indicator;
        }
    }
}
