using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberOverlay : GraphOverlay<CyberGraph, CyberNode, CyberNodeIndicator> {
    public override void SetEdgeGraphicState() {
        base.SetEdgeGraphicState();
        foreach (HashSet<string> edge in graph.edgePairs) {
            LineRenderer renderer = GetLineRenderer(edge);
            string[] nodes = edge.ToArray();
            CyberNode node1 = graph.nodes[nodes[0]];
            CyberNode node2 = graph.nodes[nodes[1]];
            if (node1.compromised && node2.compromised) {
                renderer.material.color = colorSet.deadColor;
            } else {
                renderer.material.color = colorSet.enabledColor;
            }
        }
    }

    public void NodeMouseOverCallback(CyberNodeIndicator indicator) {
        if (GameManager.I.IsCyberNodeVulnerable(indicator.node)) {
            foreach (HashSet<string> edge in graph.edgePairs) {
                string[] nodes = edge.ToArray();
                CyberNode node1 = graph.nodes[nodes[0]];
                CyberNode node2 = graph.nodes[nodes[1]];
                if (indicator.node == node1 || indicator.node == node2) {
                    if (node1.compromised || node2.compromised) {
                        LineRenderer renderer = GetLineRenderer(edge);
                        renderer.material.color = colorSet.deadColor;
                    }
                }
            }
        }
    }
    public void NodeMouseExitCallback(CyberNodeIndicator indicator) {
        SetEdgeGraphicState();
    }
}
