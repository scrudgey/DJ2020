using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberOverlay : GraphOverlay<CyberGraph, CyberNode, NeoCyberNodeIndicator> {
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;

    HashSet<string> neighborEdge;

    public override void SetEdgeGraphicState() {
        base.SetEdgeGraphicState();
        foreach (HashSet<string> edge in graph.edgePairs) {
            SetEdgeState(edge);
        }
    }

    void SetEdgeState(HashSet<string> edge) {
        LineRenderer renderer = GetLineRenderer(edge);
        string[] nodes = edge.ToArray();
        CyberNode node1 = graph.nodes[nodes[0]];
        CyberNode node2 = graph.nodes[nodes[1]];

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


    public void NeighborButtonClick(string idn) {
        NeighborButtonMouseExit();

        CyberNode node = graph.nodes[idn];
        NeoCyberNodeIndicator indicator = GetIndicator(node);
        overlayHandler.NodeSelectCallback(indicator);
    }

    public void NeighborButtonMouseOver(string sourceidn, string neighboridn) {
        HashSet<string> edge = new HashSet<string> { sourceidn, neighboridn };
        LineRenderer renderer = GetLineRenderer(edge);
        renderer.material.color = Color.white;
        neighborEdge = edge;
    }
    public void NeighborButtonMouseExit() {
        if (neighborEdge != null) {
            SetEdgeState(neighborEdge);
        }
        // HashSet<string> edge = new HashSet<string> { sourceidn, neighboridn };
    }
}
