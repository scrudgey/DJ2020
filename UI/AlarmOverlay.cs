using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlarmOverlay : GraphOverlay<AlarmGraph, AlarmNode, AlarmNodeIndicator> {
    public override void SetEdgeState(LineRenderer renderer, AlarmNode node1, AlarmNode node2) {
        renderer.material.color = colorSet.disabledColor;
        if (edgeIsActive(node1, node2)) {
            renderer.material.color = colorSet.enabledColor;
        }
    }

    bool edgeIsActive(AlarmNode node1, AlarmNode node2) {
        HashSet<string> edge = new HashSet<string> { node1.idn, node2.idn };
        AlarmGraph alarmGraph = (AlarmGraph)graph;
        return alarmGraph.activeEdges.Any(activeEdge => activeEdge.All(idn => edge.Contains(idn)));
    }
}
