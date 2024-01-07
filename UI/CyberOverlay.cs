using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberOverlay : GraphOverlay<CyberGraph, CyberNode, NeoCyberNodeIndicator> {
    public RectTransform cyberdeckIndicator;
    public LineRenderer cyberdeckLineRenderer;
    public Material normalCyberdeckLineMaterial;
    public Material marchingAntsCyberdeckLineMaterial;
    public Color marchingAntsColor;
    [Header("sound effects")]
    public AudioClip[] finishDownloadSound;
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;
    public Color dimInvulnerableColor;
    public Color dimVulnerableColor;
    public Color dimCompromisedColor;

    float marchingAntsTimer;

    bool subscribed;
    void OnDestroy() {
        if (subscribed) {
            graph.NetworkActionComplete -= HandleCompleteNetworkAction;
            graph.NetworkActionsChanged -= HandleNetworkActionsChange;
        }
    }

    public override void UpdateNodes() {
        marchingAntsTimer += Time.fixedDeltaTime;
        if (!subscribed) {
            subscribed = true;
            graph.NetworkActionComplete += HandleCompleteNetworkAction;
            graph.NetworkActionsChanged += HandleNetworkActionsChange;
        }
        if (GameManager.I.activeOverlayType == OverlayType.cyber) {
            base.UpdateNodes();
        } else if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            LimitedUpdate();
        }

        SetPlayerNodeIndicator();

        DrawMarchingAntsForAllActiveActions();
    }
    void SetPlayerNodeIndicator() {
        if (GameManager.I.playerManualHacker.deployed && GameManager.I.activeOverlayType == OverlayType.cyber) {
            // GameManager.I.playerManualHacker.lineRenderer.enabled = false;
            Vector3 playerPosition = GameManager.I.playerManualHacker.transform.position;
            Vector3 screenPoint = cam.WorldToScreenPoint(playerPosition);

            cyberdeckIndicator.gameObject.SetActive(true);
            cyberdeckIndicator.position = screenPoint;
            cyberdeckLineRenderer.material.SetTextureOffset("_MainTex", new Vector2(-1f * marchingAntsTimer, 0));


            // if (GameManager.I.playerManualHacker.targetNode != null) {
            //     List<Vector3> points = new List<Vector3>();
            //     points.Add(GameManager.I.playerManualHacker.transform.position);
            //     points.Add(GameManager.I.playerManualHacker.targetNode.position);

            //     cyberdeckLineRenderer.enabled = true;
            //     cyberdeckLineRenderer.positionCount = points.Count;
            //     cyberdeckLineRenderer.SetPositions(points.ToArray());

            // if (graph.ActiveNetworkActions().Any(networkAction => networkAction.fromPlayerNode)) {
            //     cyberdeckLineRenderer.material = marchingAntsCyberdeckLineMaterial;
            //     cyberdeckLineRenderer.material.color = marchingAntsColor;
            //     cyberdeckLineRenderer.material.SetTextureOffset("_MainTex", new Vector2(-1f * marchingAntsTimer, 0));
            // } else {
            //     cyberdeckLineRenderer.material = normalCyberdeckLineMaterial;
            //     if (GameManager.I.playerManualHacker.targetNode.getStatus() == CyberNodeStatus.compromised) {
            //         cyberdeckLineRenderer.material.color = compromisedColor;
            //     } else {
            //         cyberdeckLineRenderer.material.color = vulnerableColor;
            //     }
            // }
            // } else {
            //     cyberdeckLineRenderer.enabled = false;
            // }
        } else {
            cyberdeckIndicator.gameObject.SetActive(false);
            // cyberdeckLineRenderer.enabled = false;
            // GameManager.I.playerManualHacker.lineRenderer.enabled = true;
        }
    }
    public override void RefreshEdgeGraphicState() {
        base.RefreshEdgeGraphicState();
        if (GameManager.I.playerManualHacker.targetNode != null) {
            List<Vector3> points = new List<Vector3>();
            points.Add(GameManager.I.playerManualHacker.transform.position);
            points.Add(GameManager.I.playerManualHacker.targetNode.position);

            cyberdeckLineRenderer.enabled = true;
            cyberdeckLineRenderer.positionCount = points.Count;
            cyberdeckLineRenderer.SetPositions(points.ToArray());
            // cyberdeckLineRenderer.material.SetTextureOffset("_MainTex", new Vector2(-1f * marchingAntsTimer, 0));

            if (graph.ActiveNetworkActions().Any(networkAction => networkAction.fromPlayerNode)) {
                cyberdeckLineRenderer.material = marchingAntsCyberdeckLineMaterial;
                cyberdeckLineRenderer.material.color = marchingAntsColor;
                // cyberdeckLineRenderer.material.SetTextureOffset("_MainTex", new Vector2(-1f * marchingAntsTimer, 0));
            } else {
                cyberdeckLineRenderer.material = normalCyberdeckLineMaterial;
                if (GameManager.I.playerManualHacker.targetNode.getStatus() == CyberNodeStatus.compromised) {
                    cyberdeckLineRenderer.material.color = compromisedColor;
                } else {
                    cyberdeckLineRenderer.material.color = vulnerableColor;
                }
            }
        } else {
            cyberdeckLineRenderer.enabled = false;
        }
    }
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
        RefreshEdgeGraphicState();
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
        RefreshEdgeGraphicState();
    }
    public override void NodeMouseExitCallback(NodeIndicator<CyberNode, CyberGraph> indicator) {
        base.NodeMouseExitCallback(indicator);
        RefreshEdgeGraphicState();
    }


    public void DrawThreat(NetworkAction networkAction) {
        if (networkAction.path.Count > 1) {
            DrawMatchingAntsOnPath(networkAction);
        }
    }
    public void EraseThreat(NetworkAction networkAction) {
        if (networkAction.path.Count > 1) {
            EraseMatchingAntsOnPath(networkAction);
            RefreshEdgeGraphicState();
        }
    }
    void DrawMarchingAntsForAllActiveActions() {
        foreach (NetworkAction action in graph.ActiveNetworkActions()) {
            if (action.path.Count < 2) continue; // no path / path to player cyberdeck
            DrawMatchingAntsOnPath(action);
        }
    }
    void HandleNetworkActionsChange(Dictionary<CyberNode, List<NetworkAction>> actions) {
        RefreshEdgeGraphicState();
    }
    void HandleCompleteNetworkAction(NetworkAction networkAction) {
        Toolbox.RandomizeOneShot(overlayHandler.audioSource, finishDownloadSound);
        if (networkAction.path.Count < 2) return; // no path / path to player cyberdeck
        EraseMatchingAntsOnPath(networkAction);
    }

    void EraseMatchingAntsOnPath(NetworkAction networkAction) {
        CyberNode currentnode = networkAction.toNode;
        for (int i = 0; i < networkAction.path.Count; i++) {
            CyberNode nextNode = networkAction.path[i];
            HashSet<string> edge = new HashSet<string> { currentnode.idn, nextNode.idn };
            LineRenderer renderer = GetLineRenderer(edge);
            renderer.material = lineRendererMaterial;
            currentnode = nextNode;
        }
        // if (networkAction.fromPlayerNode) {

        // }
    }
    void DrawMatchingAntsOnPath(NetworkAction action) {
        CyberNode currentnode = action.toNode;
        for (int i = 0; i < action.path.Count; i++) {
            CyberNode nextNode = action.path[i];
            NeoCyberNodeIndicator indicator = indicators[nextNode];
            HashSet<string> edge = new HashSet<string> { currentnode.idn, nextNode.idn };
            LineRenderer renderer = GetLineRenderer(edge);
            renderer.material = marchingAntsMaterial;
            indicator.lineMaterial.SetTextureOffset("_MainTex", new Vector2(-2f * action.timer, 0));
            currentnode = nextNode;
        }
        // if (action.fromPlayerNode) {

        // }
    }
}
