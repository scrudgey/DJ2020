using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: abstract this out
public class CyberOverlay : MonoBehaviour {
    public UIColorSet colorSet;
    public Camera cam;
    public GameObject nodeIndicatorPrefab;
    CyberGraph graph;

    Dictionary<CyberNode, CyberNodeIndicator> indicators = new Dictionary<CyberNode, CyberNodeIndicator>();
    void Awake() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
    public void DisableOverlay() {
        indicators = new Dictionary<CyberNode, CyberNodeIndicator>();
    }

    void Update() {
        foreach (KeyValuePair<CyberNode, CyberNodeIndicator> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            kvp.Value.Configure(kvp.Key, graph);
            kvp.Value.SetScreenPosition(screenPoint);
        }
    }
    public void Refresh(CyberGraph graph) {
        this.graph = graph;
        Debug.Log($"refreshing cyber graph {graph} {cam}");
        if (graph == null || cam == null) {
            DisableOverlay();
            return;
        }

        foreach (CyberNode node in graph.nodes.Values) {
            Debug.Log($"node: {node}");
            Vector3 screenPoint = cam.WorldToScreenPoint(node.position);
            CyberNodeIndicator indicator = GetIndicator(node);
            indicator.Configure(node, graph);
            indicator.SetScreenPosition(screenPoint);
        }
    }

    CyberNodeIndicator GetIndicator(CyberNode node) {
        if (indicators.ContainsKey(node)) {
            return indicators[node];
        } else {
            GameObject newIndicator = GameObject.Instantiate(nodeIndicatorPrefab);
            CyberNodeIndicator indicator = newIndicator.GetComponent<CyberNodeIndicator>();
            indicator.Configure(node, graph);
            indicator.rectTransform.SetParent(transform, false);
            indicators[node] = indicator;
            return indicator;
        }
    }
}
