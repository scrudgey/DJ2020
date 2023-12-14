using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeInfoPaneDisplay : MonoBehaviour {
    public TextMeshProUGUI title;
    public TextMeshProUGUI type;
    public TextMeshProUGUI status;
    public TextMeshProUGUI lockStatus;
    public Image icon;
    public NodeDataInfoDisplay dataInfoDisplay;
    public NodeUtilityInterfaceDisplay utilityInterfaceDisplay;
    public GameObject neighborContainer;
    public Transform neighborbuttonContainer;
    public GameObject neighborButtonPrefab;
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;
    CyberNode node;
    CyberOverlay handler;

    // TODO: support visibility
    // TODO: support utility node
    public void ConfigureCyberNode(NeoCyberNodeIndicator indicator, CyberGraph graph, CyberOverlay handler) {
        this.node = indicator.node;
        this.handler = handler;

        title.text = node.nodeTitle;
        type.text = $"{node.type}";
        status.text = $"{node.status}";
        lockStatus.text = $"lock: {node.lockLevel}";
        icon.sprite = indicator.iconImage.sprite;

        Color statusColor = node.status switch {
            CyberNodeStatus.invulnerable => invulnerableColor,
            CyberNodeStatus.vulnerable => vulnerableColor,
            CyberNodeStatus.compromised => compromisedColor,
            _ => invulnerableColor
        };
        icon.color = statusColor;
        title.color = statusColor;
        type.color = statusColor;
        status.color = statusColor;

        if (node.type == CyberNodeType.datanode && node.payData != null) {
            dataInfoDisplay.Configure(node.payData);
            dataInfoDisplay.gameObject.SetActive(true);
            utilityInterfaceDisplay.gameObject.SetActive(false);
        } else if (node.type == CyberNodeType.utility) {
            utilityInterfaceDisplay.Configure(node);
            dataInfoDisplay.gameObject.SetActive(false);
            utilityInterfaceDisplay.gameObject.SetActive(true);
        } else {
            dataInfoDisplay.gameObject.SetActive(false);
            utilityInterfaceDisplay.gameObject.SetActive(false);
        }

        foreach (Transform child in neighborbuttonContainer) {
            Destroy(child.gameObject);
        }
        if (graph.edges.ContainsKey(node.idn) && graph.edges[node.idn].Count > 0) {
            foreach (string neighborId in graph.edges[node.idn]) {
                GameObject obj = GameObject.Instantiate(neighborButtonPrefab) as GameObject;
                obj.transform.SetParent(neighborbuttonContainer, false);
                NeighborButton button = obj.GetComponent<NeighborButton>();
                button.Configure(this, graph.nodes[neighborId]);
            }
            neighborContainer.SetActive(true);
        } else {
            neighborContainer.SetActive(false);
        }
    }

    public void NeighborButtonClick(string idn) {
        handler.NeighborButtonClick(idn);
    }
    public void NeighborButtonMouseOver(string idn) {
        handler.NeighborButtonMouseOver(node.idn, idn);
    }
    public void NeighborButtonMouseExit(string idn) {
        handler.NeighborButtonMouseExit();
    }

    public void MouseOverScrollBox() {
        handler.overlayHandler.uIController.mouseOverScrollBox = true;
    }
    public void MouseExitScrollBox() {
        handler.overlayHandler.uIController.mouseOverScrollBox = false;
    }
}
