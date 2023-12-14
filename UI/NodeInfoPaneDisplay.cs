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
    public GameObject neighborContainer;
    public Transform neighborbuttonContainer;
    public GameObject neighborButtonPrefab;
    CyberNode node;
    CyberOverlay handler;

    // TODO: support visibility
    // TODO: support utility node
    public void ConfigureCyberNode(CyberNode node, CyberGraph graph, CyberOverlay handler) {
        this.node = node;
        this.handler = handler;

        title.text = node.nodeTitle;
        type.text = $"{node.type}";
        status.text = $"{node.status}";
        lockStatus.text = $"lock: {node.lockLevel}";
        if (node.type == CyberNodeType.datanode && node.payData != null) {
            dataInfoDisplay.Configure(node.payData);
            dataInfoDisplay.gameObject.SetActive(true);
        } else {
            dataInfoDisplay.gameObject.SetActive(false);
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
        handler.NeighborButtonMouseExit(node.idn, idn);
    }
}
