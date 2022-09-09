using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlarmOverlay : GraphOverlay<AlarmGraph, AlarmNode, AlarmNodeIndicator> {
    public override void SetEdgeGraphicState() {
        base.SetEdgeGraphicState();

        AlarmGraph alarmGraph = (AlarmGraph)graph;

        foreach (HashSet<string> edge in graph.edgePairs) {
            LineRenderer renderer = GetLineRenderer(edge);
            renderer.material.color = colorSet.disabledColor;

            if (edgeIsActive(edge)) {
                renderer.material.color = colorSet.enabledColor;
            }
        }
    }

    bool edgeIsActive(HashSet<string> edge) {
        AlarmGraph alarmGraph = (AlarmGraph)graph;
        return alarmGraph.activeEdges.Any(activeEdge => activeEdge.All(idn => edge.Contains(idn)));
    }
}
