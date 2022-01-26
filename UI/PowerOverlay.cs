using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PowerOverlay : MonoBehaviour {
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
            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            PowerNodeIndicator indicator = GetIndicator(node);
            indicator.Configure(node, graph);
            indicator.SetScreenPosition(screenPoint);
        }
    }

    PowerNodeIndicator GetIndicator(PowerNode node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            GameObject newIndicator = GameObject.Instantiate(powerNodeIndicatorPrefab);
            PowerNodeIndicator indicator = newIndicator.GetComponent<PowerNodeIndicator>();
            indicator.Configure(node, graph);
            indicator.rectTransform.SetParent(transform, false);
            indicators[node] = indicator;
            return indicator;
        }
    }
}
