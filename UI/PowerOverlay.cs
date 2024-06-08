using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PowerOverlay : GraphOverlay<PowerGraph, PowerNode, PowerNodeIndicator> {

    override public void SetEdgeState(LineRenderer renderer, PowerNode node1, PowerNode node2) {
        if (graph.disabledEdges.Contains((node1.idn, node2.idn))) {
            renderer.material.color = Color.red;
        } else if (node1.powered && node2.powered) {
            renderer.material.color = colorSet.enabledColor;
        } else {
            renderer.material.color = colorSet.disabledColor;
        }
    }
}
