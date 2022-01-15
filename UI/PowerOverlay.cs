using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PowerOverlay : MonoBehaviour {
    public Camera cam;
    public GameObject powerNodeIndicatorPrefab;
    public LineRenderer lineRenderer;

    public void DisableOverlay() {
        lineRenderer.enabled = false;
    }

    public void Refresh(PowerGraph graph) {

        if (graph == null) {
            DisableOverlay();
            return;
        }

        List<Vector3> positions = new List<Vector3>();
        foreach (PowerNode node in graph.nodes.Values) {
            foreach (PowerNode neighbor in graph.Neighbors(node)) {
                positions.Add(node.position);
                positions.Add(neighbor.position);
            }
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }
}
