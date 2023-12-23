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
        if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            renderer.enabled = false;
            return;
        }
        renderer.enabled = true;
        bool doHighlight = mousedOverIndicator != null && (mousedOverIndicator.node == node1 || mousedOverIndicator.node == node2);
        NeoCyberNodeIndicator indicator1 = GetIndicator(node1);
        NeoCyberNodeIndicator indicator2 = GetIndicator(node2);
        if (overlayHandler.selectedNode == indicator1 || overlayHandler.selectedNode == indicator2) {
            doHighlight = true;
        }

        if (node1.getStatus() == CyberNodeStatus.vulnerable && node2.getStatus() == CyberNodeStatus.compromised) {
            renderer.material.color = doHighlight ? vulnerableColor : dimVulnerableColor;
        } else if (node2.getStatus() == CyberNodeStatus.vulnerable && node1.getStatus() == CyberNodeStatus.compromised) {
            renderer.material.color = doHighlight ? vulnerableColor : dimVulnerableColor;
        } else if (node2.getStatus() == CyberNodeStatus.compromised && node1.getStatus() == CyberNodeStatus.compromised) {
            renderer.material.color = doHighlight ? compromisedColor : dimCompromisedColor;
        } else {
            renderer.material.color = doHighlight ? invulnerableColor : dimInvulnerableColor;
        }
    }
    public override void OnOverlayActivate() {
        SetEdgeGraphicState();
    }
    public override void UpdateNodes() {
        if (GameManager.I.activeOverlayType == OverlayType.cyber) {
            base.UpdateNodes();
        } else if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            LimitedUpdate();
        }
    }
    void LimitedUpdate() {
        Vector3 playerPosition = GameManager.I.playerPosition;
        foreach (KeyValuePair<CyberNode, NeoCyberNodeIndicator> kvp in indicators) {
            Vector3 screenPoint = cam.WorldToScreenPoint(kvp.Key.position);
            kvp.Value.SetScreenPosition(screenPoint);
            float distance = Vector3.Distance(kvp.Key.position, playerPosition);
            if (distance < 3) {
                kvp.Value.gameObject.SetActive(true);
                kvp.Value.SetGraphicalState(kvp.Key);
            } else {
                kvp.Value.gameObject.SetActive(false);
            }
        }
    }
    public override void NodeMouseOverCallback(NodeIndicator<CyberNode, CyberGraph> indicator) {
        base.NodeMouseOverCallback(indicator);
        SetEdgeGraphicState();
        // this code properly belongs somewhere else.
        // if (GameManager.I.IsCyberNodeVulnerable(indicator.node)) {
        //     foreach (HashSet<string> edge in graph.edgePairs) {
        //         string[] nodes = edge.ToArray();
        //         CyberNode node1 = graph.nodes[nodes[0]];
        //         CyberNode node2 = graph.nodes[nodes[1]];
        //         if (indicator.node == node1 || indicator.node == node2) {
        //             if (node1.compromised || node2.compromised) {
        //                 LineRenderer renderer = GetLineRenderer(edge);
        //                 renderer.material.color = colorSet.deadColor;
        //             }
        //         }
        //     }
        // }
    }
    public override void NodeMouseExitCallback(NodeIndicator<CyberNode, CyberGraph> indicator) {
        base.NodeMouseExitCallback(indicator);
        SetEdgeGraphicState();
    }


}
