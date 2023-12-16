using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberOverlay : GraphOverlay<CyberGraph, CyberNode, NeoCyberNodeIndicator> {
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;
    public Color dimInvulnerableColor;
    public Color dimVulnerableColor;
    public Color dimCompromisedColor;

    override public void SetEdgeState(LineRenderer renderer, CyberNode node1, CyberNode node2) {
        bool doHighlight = mousedOverIndicator != null && (mousedOverIndicator.node == node1 || mousedOverIndicator.node == node2);
        NeoCyberNodeIndicator indicator1 = GetIndicator(node1);
        NeoCyberNodeIndicator indicator2 = GetIndicator(node2);
        if (overlayHandler.selectedNode == indicator1 || overlayHandler.selectedNode == indicator2) {
            doHighlight = true;
        }

        if (node1.status == CyberNodeStatus.vulnerable && node2.status == CyberNodeStatus.compromised) {
            renderer.material.color = doHighlight ? vulnerableColor : dimVulnerableColor;
        } else if (node2.status == CyberNodeStatus.vulnerable && node1.status == CyberNodeStatus.compromised) {
            renderer.material.color = doHighlight ? vulnerableColor : dimVulnerableColor;
        } else if (node2.status == CyberNodeStatus.compromised && node1.status == CyberNodeStatus.compromised) {
            renderer.material.color = doHighlight ? compromisedColor : dimCompromisedColor;
        } else {
            renderer.material.color = doHighlight ? invulnerableColor : dimInvulnerableColor;
        }
    }

    public override void NodeMouseOverCallback(NodeIndicator<CyberNode, CyberGraph> indicator) {
        base.NodeMouseOverCallback(indicator);
        SetEdgeGraphicState();
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
    public override void NodeMouseExitCallback(NodeIndicator<CyberNode, CyberGraph> indicator) {
        base.NodeMouseExitCallback(indicator);
        SetEdgeGraphicState();
    }


}
