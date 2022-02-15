using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: abstract this out
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
}
