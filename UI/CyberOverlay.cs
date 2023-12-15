using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberOverlay : GraphOverlay<CyberGraph, CyberNode, NeoCyberNodeIndicator> {
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;

    override public void SetEdgeState(LineRenderer renderer, CyberNode node1, CyberNode node2) {
        if (node1.status == CyberNodeStatus.vulnerable && node2.status == CyberNodeStatus.compromised) {
            renderer.material.color = vulnerableColor;
        } else if (node2.status == CyberNodeStatus.vulnerable && node1.status == CyberNodeStatus.compromised) {
            renderer.material.color = vulnerableColor;
        } else if (node2.status == CyberNodeStatus.compromised && node1.status == CyberNodeStatus.compromised) {
            renderer.material.color = compromisedColor;
        } else {
            renderer.material.color = invulnerableColor;
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
