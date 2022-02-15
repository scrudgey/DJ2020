using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PowerOverlay : GraphOverlay<PowerGraph, PowerNode, PowerNodeIndicator> {

    public override void SetEdgeGraphicState() {
        base.SetEdgeGraphicState();
        foreach (HashSet<string> edge in graph.edgePairs) {
            LineRenderer renderer = GetLineRenderer(edge);
            string[] nodes = edge.ToArray();
            PowerNode node1 = graph.nodes[nodes[0]];
            PowerNode node2 = graph.nodes[nodes[1]];

            if (node1.powered || node2.powered) {
                renderer.material.color = colorSet.enabledColor;
            } else {
                renderer.material.color = colorSet.disabledColor;
            }
        }
    }
}
